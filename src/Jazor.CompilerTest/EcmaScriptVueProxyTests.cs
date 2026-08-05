using System.Reflection;
using ECMAScript;
using ECMAScript.Contract;
using ECMAScript.ElementPlus;
using ECMAScript.TDesign;
using ECMAScript.Vuetify;
using Microsoft.AspNetCore.Components;
using static ECMAScript.Vue3;

namespace Jazor.ComplierTest;

// Verifies proxy API contracts shared by Vue, component-library, and native-union bindings.
// 验证 Vue、组件库和原生 union 绑定共享的代理 API 合同，防止 authoring 表面回退为弱类型包装。
#pragma warning disable CA1416

[TestClass]
public sealed class EcmaScriptVueProxyTests
{
    [TestMethod]
    public void Vue_CoreProxyMethods_DoNotExposeObject()
    {
        var proxyTypes = new[] { typeof(Vue3), typeof(VueApp), typeof(VueSetupContext), typeof(VueWatchHandle) };

        foreach (var method in proxyTypes.SelectMany(static type =>
            type.GetMethods(BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance | BindingFlags.DeclaredOnly)
                .Where(static method => !method.IsSpecialName)))
        {
            AssertNotObject(method.ReturnType, $"{method.DeclaringType?.Name}.{method.Name} return");
            foreach (var parameter in method.GetParameters())
                AssertNotObject(parameter.ParameterType, $"{method.DeclaringType?.Name}.{method.Name}({parameter.Name})");
        }
    }

    [TestMethod]
    public void Vuetify_CreateVuetify_UsesStronglyTypedOptionsAndPlugin()
    {
        var overloads = typeof(Vuetify)
            .GetMethods(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly)
            .Where(static method => method.Name == nameof(Vuetify.CreateVuetify))
            .OrderBy(static method => method.GetParameters().Length)
            .ToArray();

        Assert.AreEqual(2, overloads.Length);
        Assert.IsTrue(overloads.All(static method => method.ReturnType == typeof(VuetifyPlugin)));

        CollectionAssert.AreEqual(
            Array.Empty<Type>(),
            overloads[0].GetParameters().Select(static parameter => parameter.ParameterType).ToArray());
        CollectionAssert.AreEqual(
            new[] { typeof(VuetifyOptions) },
            overloads[1].GetParameters().Select(static parameter => parameter.ParameterType).ToArray());

        Assert.IsTrue(typeof(VuePlugin).IsAssignableFrom(typeof(VuetifyPlugin)));
        Assert.IsTrue(typeof(VuePluginOptions).IsAssignableFrom(typeof(VuetifyOptions)));
    }

    [TestMethod]
    public void Vuetify_ImportHosts_UseEcmaScriptImports_InsteadOfModuleEntryMarkers()
    {
        AssertEcmaScriptImport(typeof(Vuetify), "npm:vuetify");
        AssertEcmaScriptImport(typeof(VuetifyComponents), "vuetify/components");
        AssertEcmaScriptImport(typeof(VuetifyLabsComponents), "vuetify/labs/components");
        AssertEcmaScriptImport(typeof(VuetifyDirectives), "vuetify/directives");
    }

    [TestMethod]
    public void Vuetify_RuntimeShapes_UseEcmaScriptSupportMarkers_WithoutModuleGeneration()
    {
        var runtimeShapes = new[]
        {
            typeof(IVuetifyComponent),
            typeof(VuetifyPlugin),
            typeof(VuetifyOptions),
            typeof(VuetifyThemeOptions),
            typeof(VuetifyThemeVariationOptions),
            typeof(VuetifyDisplayOptions),
            typeof(VuetifyDisplayBreakpoint),
            typeof(VuetifyDisplayThresholds),
            typeof(VuetifyIconOptions),
            typeof(VuetifyLocaleOptions),
            typeof(VuetifyDateOptions),
            typeof(VuetifyComponentRegistry),
            typeof(VuetifyLabsComponentRegistry),
            typeof(VuetifyDirectiveRegistry),
            typeof(VuetifyDirective)
        };

        foreach (var type in runtimeShapes)
            AssertEcmaScriptSupport(type);
    }

    [TestMethod]
    public void TDesign_ImportHosts_UseEcmaScriptImports_InsteadOfModuleEntryMarkers()
    {
        AssertEcmaScriptImport(typeof(TDesign), "npm:tdesign-vue-next");
        AssertEcmaScriptImport(typeof(TDesignComponents), "tdesign-vue-next");
    }

    [TestMethod]
    public void TDesign_RuntimeShapes_UseEcmaScriptSupportMarkers_WithoutModuleGeneration()
    {
        var runtimeShapes = new[]
        {
            typeof(ITDesignComponent),
            typeof(TDesignPlugin),
            typeof(TDesignInstallOptions),
            typeof(TDesignGlobalConfig),
            typeof(TDesignComponentRegistry),
            typeof(TDesignStyles),
            typeof(TDesignMenuQueryData),
            typeof(TDesignMenuRoute),
            typeof(TDesignMenuItemClickContext),
            typeof(TDesignTabAddContext),
            typeof(TDesignTabRemoveContext),
            typeof(TDesignTabPanelRemoveContext),
            typeof(TDesignTabsDragSortContext),
            typeof(TDesignAvatarErrorContext)
        };

        foreach (var type in runtimeShapes)
            AssertEcmaScriptSupport(type);
    }

    [TestMethod]
    public void TDesign_PluginAndInstallSurface_UsesStronglyTypedContracts()
    {
        var defaultExport = typeof(TDesign).GetProperty(nameof(TDesign.Default), BindingFlags.Public | BindingFlags.Static);
        var version = typeof(TDesign).GetProperty(nameof(TDesign.Version), BindingFlags.Public | BindingFlags.Static);
        var installOverloads = typeof(TDesign)
            .GetMethods(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly)
            .Where(static method => method.Name == nameof(TDesign.Install))
            .OrderBy(static method => method.GetParameters().Length)
            .ToArray();

        Assert.IsNotNull(defaultExport);
        Assert.AreEqual(typeof(TDesignPlugin), defaultExport!.PropertyType);
        Assert.AreEqual("default", defaultExport.GetCustomAttribute<ECMAScriptNameAttribute>()?.Name);

        Assert.IsNotNull(version);
        Assert.AreEqual(typeof(string), version!.PropertyType);

        Assert.AreEqual(2, installOverloads.Length);
        CollectionAssert.AreEqual(
            new[] { typeof(VueApp) },
            installOverloads[0].GetParameters().Select(static parameter => parameter.ParameterType).ToArray());
        CollectionAssert.AreEqual(
            new[] { typeof(VueApp), typeof(TDesignInstallOptions) },
            installOverloads[1].GetParameters().Select(static parameter => parameter.ParameterType).ToArray());

        Assert.IsTrue(typeof(VuePlugin).IsAssignableFrom(typeof(TDesignPlugin)));
        Assert.IsTrue(typeof(VuePluginOptions).IsAssignableFrom(typeof(TDesignInstallOptions)));
        Assert.AreEqual(typeof(string), typeof(TDesignGlobalConfig).GetProperty(nameof(TDesignGlobalConfig.ClassPrefix), BindingFlags.Public | BindingFlags.Instance)!.PropertyType.UnwrapNullable());
    }

    [TestMethod]
    public void Vue_ComponentOptions_UseNamedRenderAndSetupDelegates()
    {
        var setup = typeof(VueComponentOptions).GetProperty(nameof(VueComponentOptions.Setup), BindingFlags.Public | BindingFlags.Instance);
        var render = typeof(VueComponentOptions).GetProperty(nameof(VueComponentOptions.Render), BindingFlags.Public | BindingFlags.Instance);
        var canonicalProps = typeof(VueComponentOptions).GetProperty(nameof(VueComponentOptions.Props), BindingFlags.Public | BindingFlags.Instance);
        var canonicalEmits = typeof(VueComponentOptions).GetProperty(nameof(VueComponentOptions.Emits), BindingFlags.Public | BindingFlags.Instance);
        var slotComponentOptions = typeof(VueSlotComponentOptions<>).MakeGenericType(typeof(TestVueSlots));
        var slotCanonicalProps = slotComponentOptions.GetProperty(nameof(VueSlotComponentOptions<TestVueSlots>.Props), BindingFlags.Public | BindingFlags.Instance);
        var slotCanonicalEmits = slotComponentOptions.GetProperty(nameof(VueSlotComponentOptions<TestVueSlots>.Emits), BindingFlags.Public | BindingFlags.Instance);

        Assert.IsNotNull(setup);
        Assert.IsNotNull(render);
        Assert.IsNotNull(canonicalProps);
        Assert.IsNotNull(canonicalEmits);
        Assert.IsNotNull(slotCanonicalProps);
        Assert.IsNotNull(slotCanonicalEmits);
        Assert.AreEqual(typeof(VueSetupCallback), setup.PropertyType);
        Assert.AreEqual(typeof(VueRenderCallback), render.PropertyType);
        Assert.AreEqual(typeof(VueNamesOrOptions), canonicalProps.PropertyType.UnwrapNullable());
        Assert.AreEqual(typeof(VueNamesOrOptions), canonicalEmits.PropertyType.UnwrapNullable());
        Assert.AreEqual(typeof(VueNamesOrOptions), slotCanonicalProps.PropertyType.UnwrapNullable());
        Assert.AreEqual(typeof(VueNamesOrOptions), slotCanonicalEmits.PropertyType.UnwrapNullable());
        Assert.IsNotNull(typeof(VueNamesOrOptions).GetCustomAttribute<System.Runtime.CompilerServices.UnionAttribute>());
        Assert.IsTrue(typeof(System.Runtime.CompilerServices.IUnion).IsAssignableFrom(typeof(VueNamesOrOptions)));
        Assert.IsTrue(typeof(System.Collections.Generic.IEnumerable<string>).IsAssignableFrom(typeof(VueNamesOrOptions)));
        Assert.IsNotNull(typeof(VueNamesOrOptions).GetProperty(nameof(System.Runtime.CompilerServices.IUnion.Value), BindingFlags.Public | BindingFlags.Instance));
        CollectionAssert.AreEquivalent(
            new[] { typeof(string[]), typeof(VueProps) },
            typeof(VueNamesOrOptions)
                .GetConstructors(BindingFlags.Public | BindingFlags.Instance)
                .Select(static constructor => constructor.GetParameters().SingleOrDefault()?.ParameterType)
                .Where(static type => type is not null)
                .ToArray());
        var collectionBuilder = typeof(VueNamesOrOptions).GetCustomAttribute<System.Runtime.CompilerServices.CollectionBuilderAttribute>();
        Assert.IsNotNull(collectionBuilder);
        Assert.AreEqual(typeof(VueNamesOrOptionsCollectionBuilder), collectionBuilder!.BuilderType);
        Assert.AreEqual(nameof(VueNamesOrOptionsCollectionBuilder.Create), collectionBuilder.MethodName);
        Assert.IsNull(typeof(VueComponentOptions).GetProperty("PropOptions", BindingFlags.Public | BindingFlags.Instance));
        Assert.IsNull(typeof(VueComponentOptions).GetProperty("PropNames", BindingFlags.Public | BindingFlags.Instance));
        Assert.IsNull(typeof(VueComponentOptions).GetProperty("EmitOptions", BindingFlags.Public | BindingFlags.Instance));
        Assert.IsNull(typeof(VueComponentOptions).GetProperty("EmitNames", BindingFlags.Public | BindingFlags.Instance));
        Assert.IsNull(slotComponentOptions.GetProperty("PropOptions", BindingFlags.Public | BindingFlags.Instance));
        Assert.IsNull(slotComponentOptions.GetProperty("PropNames", BindingFlags.Public | BindingFlags.Instance));
        Assert.IsNull(slotComponentOptions.GetProperty("EmitOptions", BindingFlags.Public | BindingFlags.Instance));
        Assert.IsNull(slotComponentOptions.GetProperty("EmitNames", BindingFlags.Public | BindingFlags.Instance));
    }

    [TestMethod]
    public void ECMAScript_BufferSourceInterfaces_ExposeAllowSharedAsBufferSource()
    {
        Assert.IsTrue(typeof(IBufferSource).IsAssignableFrom(typeof(IAllowSharedBufferSource)));
        Assert.IsTrue(typeof(IAllowSharedBufferSource).IsAssignableFrom(typeof(ArrayBuffer)));
        Assert.IsTrue(typeof(IAllowSharedBufferSource).IsAssignableFrom(typeof(SharedArrayBuffer)));
        Assert.IsTrue(typeof(IAllowSharedBufferSource).IsAssignableFrom(typeof(Uint8Array)));
    }

    [TestMethod]
    public void Vue_SetupContext_Emit_ExposesTypedMultiPayloadOverloads()
    {
        var overloads = typeof(VueSetupContext)
            .GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
            .Where(static method => method.Name == nameof(VueSetupContext.Emit))
            .ToArray();
        var modelEmitOverload = overloads.Single(static method =>
            method.IsGenericMethodDefinition &&
            method.GetGenericArguments().Length == 2 &&
            method.GetParameters().Length == 2 &&
            method.GetParameters()[0].ParameterType.IsGenericType &&
            method.GetParameters()[0].ParameterType.GetGenericTypeDefinition() == typeof(VueModelName<,>) &&
            method.GetCustomAttribute<ECMAScriptInlineAttribute>()?.RawFuncCode == "__arg1.emit(`update:${__arg2}`, __arg3)");

        static bool HasPayloadOverload(MethodInfo[] overloads, int genericArity, int parameterCount)
            => overloads.Any(method =>
                method.ReturnType == typeof(void) &&
                method.GetGenericArguments().Length == genericArity &&
                method.GetParameters().Length == parameterCount &&
                method.GetParameters()[0].ParameterType == typeof(string));

        Assert.IsTrue(HasPayloadOverload(overloads, 0, 1));
        Assert.IsTrue(HasPayloadOverload(overloads, 1, 2));
        Assert.IsTrue(HasPayloadOverload(overloads, 2, 3));
        Assert.IsTrue(HasPayloadOverload(overloads, 3, 4));
        Assert.IsTrue(HasPayloadOverload(overloads, 4, 5));
        Assert.AreEqual(typeof(void), modelEmitOverload.ReturnType);

        AssertNotObject(typeof(VueSetupContext), nameof(VueSetupContext));
    }

    [TestMethod]
    public void Vue_SpreadAttribute_And_VueObject_Surface_ArePublicAndTyped()
    {
        var spreadUsage = typeof(SpreadAttribute).GetCustomAttribute<AttributeUsageAttribute>();
        var vueObjectType = typeof(VueObject);
        var typedVueObjectType = typeof(VueObject<>);
        var vueDictionaryType = typeof(VueDictionary<>);
        var vueDictionary = typeof(VueDictionary);
        var vueValue = typeof(VueValue);
        var vueKey = typeof(VueKey);
        var isProperty = typeof(VueObject).GetProperty("Is", BindingFlags.Public | BindingFlags.Instance);
        var key = typeof(VueObject).GetProperty(nameof(VueObject.Key), BindingFlags.Public | BindingFlags.Instance);
        var @ref = typeof(VueObject).GetProperty(nameof(VueObject.Ref), BindingFlags.Public | BindingFlags.Instance);
        var @for = typeof(VueObject).GetProperty(nameof(VueObject.For), BindingFlags.Public | BindingFlags.Instance);
        var spellcheck = typeof(VueObject).GetProperty(nameof(VueObject.Spellcheck), BindingFlags.Public | BindingFlags.Instance);
        var rows = typeof(VueObject).GetProperty(nameof(VueObject.Rows), BindingFlags.Public | BindingFlags.Instance);
        var cols = typeof(VueObject).GetProperty(nameof(VueObject.Cols), BindingFlags.Public | BindingFlags.Instance);
        var value = typeof(VueObject).GetProperty(nameof(VueObject.Value), BindingFlags.Public | BindingFlags.Instance);
        var min = typeof(VueObject).GetProperty(nameof(VueObject.Min), BindingFlags.Public | BindingFlags.Instance);
        var max = typeof(VueObject).GetProperty(nameof(VueObject.Max), BindingFlags.Public | BindingFlags.Instance);
        var step = typeof(VueObject).GetProperty(nameof(VueObject.Step), BindingFlags.Public | BindingFlags.Instance);
        var minlength = typeof(VueObject).GetProperty(nameof(VueObject.Minlength), BindingFlags.Public | BindingFlags.Instance);
        var maxLength = typeof(VueObject).GetProperty(nameof(VueObject.Maxlength), BindingFlags.Public | BindingFlags.Instance);
        var pattern = typeof(VueObject).GetProperty(nameof(VueObject.Pattern), BindingFlags.Public | BindingFlags.Instance);
        var accept = typeof(VueObject).GetProperty(nameof(VueObject.Accept), BindingFlags.Public | BindingFlags.Instance);
        var wrap = typeof(VueObject).GetProperty(nameof(VueObject.Wrap), BindingFlags.Public | BindingFlags.Instance);
        var name = typeof(VueObject).GetProperty(nameof(VueObject.Name), BindingFlags.Public | BindingFlags.Instance);
        var type = typeof(VueObject).GetProperty(nameof(VueObject.Type), BindingFlags.Public | BindingFlags.Instance);
        var placeholder = typeof(VueObject).GetProperty(nameof(VueObject.Placeholder), BindingFlags.Public | BindingFlags.Instance);
        var autocomplete = typeof(VueObject).GetProperty(nameof(VueObject.Autocomplete), BindingFlags.Public | BindingFlags.Instance);
        var autofocus = typeof(VueObject).GetProperty(nameof(VueObject.Autofocus), BindingFlags.Public | BindingFlags.Instance);
        var disabled = typeof(VueObject).GetProperty(nameof(VueObject.Disabled), BindingFlags.Public | BindingFlags.Instance);
        var @checked = typeof(VueObject).GetProperty(nameof(VueObject.Checked), BindingFlags.Public | BindingFlags.Instance);
        var readOnly = typeof(VueObject).GetProperty(nameof(VueObject.Readonly), BindingFlags.Public | BindingFlags.Instance);
        var required = typeof(VueObject).GetProperty(nameof(VueObject.Required), BindingFlags.Public | BindingFlags.Instance);
        var multiple = typeof(VueObject).GetProperty(nameof(VueObject.Multiple), BindingFlags.Public | BindingFlags.Instance);
        var selected = typeof(VueObject).GetProperty(nameof(VueObject.Selected), BindingFlags.Public | BindingFlags.Instance);
        var tabindex = typeof(VueObject).GetProperty(nameof(VueObject.Tabindex), BindingFlags.Public | BindingFlags.Instance);
        var role = typeof(VueObject).GetProperty(nameof(VueObject.Role), BindingFlags.Public | BindingFlags.Instance);
        var href = typeof(VueObject).GetProperty(nameof(VueObject.Href), BindingFlags.Public | BindingFlags.Instance);
        var target = typeof(VueObject).GetProperty(nameof(VueObject.Target), BindingFlags.Public | BindingFlags.Instance);
        var rel = typeof(VueObject).GetProperty(nameof(VueObject.Rel), BindingFlags.Public | BindingFlags.Instance);
        var src = typeof(VueObject).GetProperty(nameof(VueObject.Src), BindingFlags.Public | BindingFlags.Instance);
        var alt = typeof(VueObject).GetProperty(nameof(VueObject.Alt), BindingFlags.Public | BindingFlags.Instance);
        var action = typeof(VueObject).GetProperty(nameof(VueObject.Action), BindingFlags.Public | BindingFlags.Instance);
        var method = typeof(VueObject).GetProperty(nameof(VueObject.Method), BindingFlags.Public | BindingFlags.Instance);
        var events = typeof(VueObject).GetProperty(nameof(VueObject.Events), BindingFlags.Public | BindingFlags.Instance);
        var attrs = typeof(VueObject).GetProperty(nameof(VueObject.Attrs), BindingFlags.Public | BindingFlags.Instance);
        var dataset = typeof(VueObject).GetProperty(nameof(VueObject.Dataset), BindingFlags.Public | BindingFlags.Instance);
        var raw = typeof(VueObject).GetProperty(nameof(VueObject.Raw), BindingFlags.Public | BindingFlags.Instance);
        var @class = typeof(VueObject).GetProperty(nameof(VueObject.Class), BindingFlags.Public | BindingFlags.Instance);
        var itemIndexers = typeof(VueObject)
            .GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(static property => property.Name == "Item")
            .ToArray();
        var props = typeof(VueObject<>).GetProperty("Props", BindingFlags.Public | BindingFlags.Instance);
        var dictionaryIndexers = vueDictionaryType
            .GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(static property => property.Name == "Item")
            .ToArray();
        var rootIndexers = vueDictionary
            .GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(static property => property.Name == "Item")
            .ToArray();
        var dictionaryAddMethods = vueDictionaryType
            .GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
            .Where(static method => method.Name == "Add")
            .ToArray();
        var dictionaryValueTypeParameter = vueDictionaryType.GetGenericArguments().Single();
        var item = itemIndexers.Single(static property =>
            property.GetIndexParameters().Select(static parameter => parameter.ParameterType).SequenceEqual(new[] { typeof(string) }));
        var symbolItem = itemIndexers.Single(static property =>
            property.GetIndexParameters().Select(static parameter => parameter.ParameterType).SequenceEqual(new[] { typeof(Symbol) }));
        var indexer = dictionaryIndexers.Single(static property =>
            property.GetIndexParameters().Select(static parameter => parameter.ParameterType).SequenceEqual(new[] { typeof(string) }));
        var symbolIndexer = dictionaryIndexers.Single(static property =>
            property.GetIndexParameters().Select(static parameter => parameter.ParameterType).SequenceEqual(new[] { typeof(Symbol) }));
        var rootIndexer = rootIndexers.Single(static property =>
            property.GetIndexParameters().Select(static parameter => parameter.ParameterType).SequenceEqual(new[] { typeof(string) }));
        var rootSymbolIndexer = rootIndexers.Single(static property =>
            property.GetIndexParameters().Select(static parameter => parameter.ParameterType).SequenceEqual(new[] { typeof(Symbol) }));

        Assert.IsNotNull(spreadUsage);
        Assert.AreEqual(AttributeTargets.Property, spreadUsage.ValidOn);
        Assert.AreEqual(false, spreadUsage.AllowMultiple);
        Assert.AreEqual("ECMAScript", typeof(SpreadAttribute).Namespace);
        Assert.IsFalse(vueObjectType.IsAbstract);
        Assert.IsFalse(typedVueObjectType.IsAbstract);
        Assert.IsFalse(vueDictionaryType.IsAbstract);
        Assert.IsFalse(vueDictionary.IsAbstract);
        Assert.IsFalse(vueValue.IsAbstract);
        Assert.IsFalse(vueKey.IsAbstract);
        Assert.IsTrue(typeof(VueProps).IsAssignableFrom(vueObjectType));
        Assert.IsTrue(typeof(VueProps).IsAssignableFrom(typedVueObjectType));
        Assert.IsTrue(typeof(VueProps).IsAssignableFrom(vueDictionaryType));
        Assert.IsTrue(typeof(VueProps).IsAssignableFrom(vueDictionary));
        Assert.IsNotNull(isProperty);
        Assert.IsNotNull(key);
        Assert.IsNotNull(@ref);
        Assert.IsNotNull(@for);
        Assert.IsNotNull(spellcheck);
        Assert.IsNotNull(rows);
        Assert.IsNotNull(cols);
        Assert.IsNotNull(value);
        Assert.IsNotNull(min);
        Assert.IsNotNull(max);
        Assert.IsNotNull(step);
        Assert.IsNotNull(minlength);
        Assert.IsNotNull(maxLength);
        Assert.IsNotNull(pattern);
        Assert.IsNotNull(accept);
        Assert.IsNotNull(wrap);
        Assert.IsNotNull(name);
        Assert.IsNotNull(type);
        Assert.IsNotNull(placeholder);
        Assert.IsNotNull(autocomplete);
        Assert.IsNotNull(autofocus);
        Assert.IsNotNull(disabled);
        Assert.IsNotNull(@checked);
        Assert.IsNotNull(readOnly);
        Assert.IsNotNull(required);
        Assert.IsNotNull(multiple);
        Assert.IsNotNull(selected);
        Assert.IsNotNull(tabindex);
        Assert.IsNotNull(role);
        Assert.IsNotNull(href);
        Assert.IsNotNull(target);
        Assert.IsNotNull(rel);
        Assert.IsNotNull(src);
        Assert.IsNotNull(alt);
        Assert.IsNotNull(action);
        Assert.IsNotNull(method);
        Assert.IsNotNull(events);
        Assert.IsNotNull(attrs);
        Assert.IsNotNull(dataset);
        Assert.IsNotNull(raw);
        Assert.IsNotNull(@class);
        Assert.IsNotNull(item);
        Assert.IsNotNull(symbolItem);
        Assert.IsNotNull(props);
        Assert.IsNotNull(indexer);
        Assert.IsNotNull(symbolIndexer);
        Assert.IsNotNull(rootIndexer);
        Assert.IsNotNull(rootSymbolIndexer);
        Assert.AreEqual(typeof(string), isProperty.PropertyType.UnwrapNullable());
        Assert.AreEqual(typeof(VueKey), key.PropertyType.UnwrapNullable());
        Assert.AreEqual(typeof(string), @ref.PropertyType.UnwrapNullable());
        Assert.AreEqual(typeof(string), @for.PropertyType.UnwrapNullable());
        Assert.AreEqual(typeof(bool), spellcheck.PropertyType.UnwrapNullable());
        Assert.AreEqual(typeof(int), rows.PropertyType.UnwrapNullable());
        Assert.AreEqual(typeof(int), cols.PropertyType.UnwrapNullable());
        Assert.AreEqual(typeof(string), value.PropertyType.UnwrapNullable());
        Assert.AreEqual(typeof(VueStringNumberValue), min.PropertyType.UnwrapNullable());
        Assert.AreEqual(typeof(VueStringNumberValue), max.PropertyType.UnwrapNullable());
        Assert.AreEqual(typeof(VueStringNumberValue), step.PropertyType.UnwrapNullable());
        Assert.AreEqual(typeof(int), minlength.PropertyType.UnwrapNullable());
        Assert.AreEqual(typeof(int), maxLength.PropertyType.UnwrapNullable());
        Assert.AreEqual(typeof(string), pattern.PropertyType.UnwrapNullable());
        Assert.AreEqual(typeof(string), accept.PropertyType.UnwrapNullable());
        Assert.AreEqual(typeof(string), wrap.PropertyType.UnwrapNullable());
        Assert.AreEqual(typeof(string), name.PropertyType.UnwrapNullable());
        Assert.AreEqual(typeof(string), type.PropertyType.UnwrapNullable());
        Assert.AreEqual(typeof(string), placeholder.PropertyType.UnwrapNullable());
        Assert.AreEqual(typeof(string), autocomplete.PropertyType.UnwrapNullable());
        Assert.AreEqual(typeof(bool), autofocus.PropertyType.UnwrapNullable());
        Assert.AreEqual(typeof(bool), disabled.PropertyType.UnwrapNullable());
        Assert.AreEqual(typeof(bool), @checked.PropertyType.UnwrapNullable());
        Assert.AreEqual(typeof(bool), readOnly.PropertyType.UnwrapNullable());
        Assert.AreEqual(typeof(bool), required.PropertyType.UnwrapNullable());
        Assert.AreEqual(typeof(bool), multiple.PropertyType.UnwrapNullable());
        Assert.AreEqual(typeof(bool), selected.PropertyType.UnwrapNullable());
        Assert.AreEqual(typeof(int), tabindex.PropertyType.UnwrapNullable());
        Assert.AreEqual(typeof(string), role.PropertyType.UnwrapNullable());
        Assert.AreEqual(typeof(string), href.PropertyType.UnwrapNullable());
        Assert.AreEqual(typeof(string), target.PropertyType.UnwrapNullable());
        Assert.AreEqual(typeof(string), rel.PropertyType.UnwrapNullable());
        Assert.AreEqual(typeof(string), src.PropertyType.UnwrapNullable());
        Assert.AreEqual(typeof(string), alt.PropertyType.UnwrapNullable());
        Assert.AreEqual(typeof(string), action.PropertyType.UnwrapNullable());
        Assert.AreEqual(typeof(string), method.PropertyType.UnwrapNullable());
        Assert.AreEqual(typeof(VueEventHandlers), events.PropertyType.UnwrapNullable());
        Assert.AreEqual(typeof(VueValue), item.PropertyType.UnwrapNullable());
        Assert.AreEqual(typeof(VueValue), symbolItem.PropertyType.UnwrapNullable());
        Assert.AreEqual(typeof(VueValue), rootIndexer.PropertyType.UnwrapNullable());
        Assert.AreEqual(typeof(VueValue), rootSymbolIndexer.PropertyType.UnwrapNullable());
        CollectionAssert.AreEqual(new[] { typeof(string) }, indexer.GetIndexParameters().Select(static parameter => parameter.ParameterType).ToArray());
        CollectionAssert.AreEqual(new[] { typeof(Symbol) }, symbolIndexer.GetIndexParameters().Select(static parameter => parameter.ParameterType).ToArray());
        CollectionAssert.AreEqual(new[] { typeof(string) }, rootIndexer.GetIndexParameters().Select(static parameter => parameter.ParameterType).ToArray());
        CollectionAssert.AreEqual(new[] { typeof(Symbol) }, rootSymbolIndexer.GetIndexParameters().Select(static parameter => parameter.ParameterType).ToArray());
        Assert.AreEqual(2, dictionaryAddMethods.Length);
        Assert.IsTrue(dictionaryAddMethods.Any(method =>
            method.GetParameters().Select(static parameter => parameter.ParameterType).SequenceEqual(new[] { typeof(string), dictionaryValueTypeParameter })));
        Assert.IsTrue(dictionaryAddMethods.Any(method =>
            method.GetParameters().Select(static parameter => parameter.ParameterType).SequenceEqual(new[] { typeof(Symbol), dictionaryValueTypeParameter })));
        CollectionAssert.Contains(
            events.CustomAttributes.Select(static attribute => attribute.AttributeType).ToArray(),
            typeof(SpreadAttribute));
        CollectionAssert.Contains(
            attrs.CustomAttributes.Select(static attribute => attribute.AttributeType).ToArray(),
            typeof(SpreadAttribute));
        CollectionAssert.Contains(
            dataset.CustomAttributes.Select(static attribute => attribute.AttributeType).ToArray(),
            typeof(SpreadAttribute));
        CollectionAssert.Contains(
            raw.CustomAttributes.Select(static attribute => attribute.AttributeType).ToArray(),
            typeof(SpreadAttribute));
        CollectionAssert.Contains(
            props.CustomAttributes.Select(static attribute => attribute.AttributeType).ToArray(),
            typeof(SpreadAttribute));
        Assert.IsNull(typeof(VueObject).GetProperty("MinLength", BindingFlags.Public | BindingFlags.Instance));
        Assert.IsNull(typeof(VueObject).GetProperty("MaxLength", BindingFlags.Public | BindingFlags.Instance));
        Assert.IsNull(typeof(VueObject).GetProperty("AutoComplete", BindingFlags.Public | BindingFlags.Instance));
        Assert.IsNull(typeof(VueObject).GetProperty("AutoFocus", BindingFlags.Public | BindingFlags.Instance));
        Assert.IsNull(typeof(VueObject).GetProperty("ReadOnly", BindingFlags.Public | BindingFlags.Instance));
        Assert.IsNull(typeof(VueObject).GetProperty("TabIndex", BindingFlags.Public | BindingFlags.Instance));
        Assert.AreEqual(typeof(VueClassValue), @class.PropertyType.UnwrapNullable());
        Assert.IsNotNull(typeof(VueClassValue).GetCustomAttribute<System.Runtime.CompilerServices.UnionAttribute>());
        Assert.IsTrue(typeof(System.Runtime.CompilerServices.IUnion).IsAssignableFrom(typeof(VueClassValue)));

        var vueKeyImplicitSources = vueKey
            .GetMethods(BindingFlags.Public | BindingFlags.Static)
            .Where(static method => method.Name == "op_Implicit" && method.ReturnType == typeof(VueKey))
            .Select(static method => method.GetParameters().Single().ParameterType)
            .ToArray();

        CollectionAssert.Contains(vueKeyImplicitSources, typeof(string));
        CollectionAssert.Contains(vueKeyImplicitSources, typeof(int));
        CollectionAssert.Contains(vueKeyImplicitSources, typeof(Number));
        CollectionAssert.Contains(vueKeyImplicitSources, typeof(Symbol));
    }

    [TestMethod]
    public void Vue_ErasedValueUnions_UseNet11UnionContract()
    {
        AssertNet11UnionContract(typeof(VueComputedValue<int>), typeof(Func<int>), typeof(VueWritableComputedOptions<int>));
        AssertNet11UnionContract(
            typeof(VueWatchDeclaration<int>),
            typeof(string),
            typeof(Action<int, int>),
            typeof(VueWatchCleanupCallback<int>),
            typeof(VueWatchHandlerOptions<int>),
            typeof(VueWatchCleanupHandlerOptions<int>),
            typeof(VueWatchNamedHandlerOptions),
            typeof(VueWatchEntries<int>));
        AssertNet11UnionContract(typeof(VueInjectFrom<int>), typeof(string), typeof(VueInjectionKey<int>), typeof(Symbol));
        AssertNet11UnionContract(typeof(VuePropDeclaration<int>), typeof(VuePropType), typeof(VuePropType[]), typeof(VuePropOptions<int>));
        AssertNet11UnionContract(typeof(VueClassValue), typeof(string), typeof(string[]), typeof(VueProps), typeof(VueValue[]));
        AssertNet11UnionContract(typeof(VueBooleanStringValue), typeof(bool), typeof(string));
        AssertNet11UnionContract(typeof(VueStringComponentValue), typeof(string), typeof(IVueComponent));
        AssertNet11UnionContract(typeof(VueStyleValue), typeof(string), typeof(VueProps), typeof(VueStyleValues));
        AssertNet11UnionContract(typeof(VueStyleValues), typeof(VueStyleValue[]));
        AssertNet11UnionContract(typeof(VueStringNumberValue), typeof(double), typeof(string));
        AssertNet11UnionContract(typeof(VueWatchDeep), typeof(bool), typeof(int));
        AssertNet11UnionContract(typeof(VueTransitionDurationValue), typeof(Number), typeof(VueTransitionDuration));
        AssertNet11UnionContract(typeof(VueKeepAliveMatch), typeof(string), typeof(RegExp), typeof(string[]), typeof(RegExp[]));
        AssertNet11UnionContract(typeof(VueIntStringValue), typeof(int), typeof(string));
        AssertNet11UnionContract(typeof(VueTeleportTarget), typeof(string), typeof(Element));
    }

    [TestMethod]
    public void Vue_VueObject_ConvenienceBoundary_ReservesAriaAndDatasetFamilies_ForBagsAndIndexer()
    {
        var vueObjectType = typeof(VueObject);
        var attrs = vueObjectType.GetProperty(nameof(VueObject.Attrs), BindingFlags.Public | BindingFlags.Instance);
        var dataset = vueObjectType.GetProperty(nameof(VueObject.Dataset), BindingFlags.Public | BindingFlags.Instance);
        var raw = vueObjectType.GetProperty(nameof(VueObject.Raw), BindingFlags.Public | BindingFlags.Instance);
        var indexer = vueObjectType
            .GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Single(static property =>
                property.Name == "Item" &&
                property.GetIndexParameters().Select(static parameter => parameter.ParameterType).SequenceEqual(new[] { typeof(string) }));
        var forbiddenConvenienceMembers = new[]
        {
            "AriaLabel",
            "AriaHidden",
            "AriaRoleDescription",
            "DataKind",
            "DataUserId",
            "DataTestId"
        };

        Assert.IsNotNull(attrs);
        Assert.IsNotNull(dataset);
        Assert.IsNotNull(raw);
        Assert.IsNotNull(indexer);

        Assert.AreEqual(typeof(VueProps), attrs!.PropertyType.UnwrapNullable());
        Assert.AreEqual(typeof(VueProps), dataset!.PropertyType.UnwrapNullable());
        Assert.AreEqual(typeof(VueProps), raw!.PropertyType.UnwrapNullable());
        Assert.AreEqual(typeof(VueValue), indexer!.PropertyType.UnwrapNullable());

        foreach (var memberName in forbiddenConvenienceMembers)
        {
            Assert.IsNull(
                vueObjectType.GetProperty(memberName, BindingFlags.Public | BindingFlags.Instance),
                $"VueObject should keep {memberName} out of first-class convenience members and route it through Attrs/Dataset/indexer instead.");
        }
    }

    [TestMethod]
    public void ElementPlus_UsesSharedVueUnionContracts_ForCommonAuthoringShapes()
    {
        Assert.AreEqual(typeof(VueStyleValue), typeof(ElementPlusComponentBase).GetProperty(nameof(ElementPlusComponentBase.CssStyle), BindingFlags.Public | BindingFlags.Instance)!.PropertyType.UnwrapNullable());
        Assert.AreEqual(typeof(VueTeleportTarget), typeof(ElementPlusLoadingOptions).GetProperty(nameof(ElementPlusLoadingOptions.Target), BindingFlags.Public | BindingFlags.Instance)!.PropertyType.UnwrapNullable());
        Assert.AreEqual(typeof(VueBooleanStringValue), typeof(ElementPlusLinkConfig).GetProperty(nameof(ElementPlusLinkConfig.Underline), BindingFlags.Public | BindingFlags.Instance)!.PropertyType.UnwrapNullable());
    }

    [TestMethod]
    public void Vue_EventHandlers_Surface_IsStringKeyedAndTyped()
    {
        var eventHandlersType = typeof(VueEventHandlers);
        var typedEventHandlersType = typeof(VueEventHandlers<>).MakeGenericType(typeof(MouseEvent));
        var eventIndexer = eventHandlersType
            .GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
            .Single(static property => property.Name == "Item");
        var typedEventIndexer = typedEventHandlersType
            .GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
            .Single(static property => property.Name == "Item");

        Assert.IsFalse(eventHandlersType.IsAbstract);
        Assert.IsFalse(typedEventHandlersType.IsAbstract);
        Assert.IsTrue(typeof(VueProps).IsAssignableFrom(eventHandlersType));
        Assert.IsTrue(eventHandlersType.IsAssignableFrom(typedEventHandlersType));
        Assert.IsNotNull(eventIndexer);
        Assert.IsNotNull(typedEventIndexer);
        Assert.AreEqual(typeof(Action), eventIndexer.PropertyType.UnwrapNullable());
        Assert.AreEqual(typeof(VueEventHandler<MouseEvent>), typedEventIndexer.PropertyType.UnwrapNullable());
        CollectionAssert.AreEqual(new[] { typeof(string) }, eventIndexer.GetIndexParameters().Select(static parameter => parameter.ParameterType).ToArray());
        CollectionAssert.AreEqual(new[] { typeof(string) }, typedEventIndexer.GetIndexParameters().Select(static parameter => parameter.ParameterType).ToArray());

        AssertNotObject(eventHandlersType, nameof(VueEventHandlers));
        AssertNotObject(typedEventHandlersType, "VueEventHandlers<MouseEvent>");
    }

    [TestMethod]
    public void Vue_PropAndEmitObjectOptions_Surface_IsTypedAndStringKeyed()
    {
        var propType = typeof(VuePropType);
        var propOptionsType = typeof(VuePropOptions<>).MakeGenericType(typeof(string));
        var propRegistryType = typeof(VuePropRegistry<>).MakeGenericType(typeof(string));
        var emitRegistryType = typeof(VueEmitRegistry<>).MakeGenericType(typeof(string));
        var propRegistryIndexer = propRegistryType.GetProperty("Item", BindingFlags.Public | BindingFlags.Instance);
        var emitRegistryIndexer = emitRegistryType.GetProperty("Item", BindingFlags.Public | BindingFlags.Instance);
        var propOptionNames = new[]
        {
            nameof(VuePropOptions<string>.Type),
            nameof(VuePropOptions<string>.Types),
            nameof(VuePropOptions<string>.Required),
            nameof(VuePropOptions<string>.Default),
            nameof(VuePropOptions<string>.DefaultFactory),
            nameof(VuePropOptions<string>.DefaultFactoryWithProps),
            nameof(VuePropOptions<string>.Validator),
            nameof(VuePropOptions<string>.ValidatorWithProps)
        };
        var propOptionProperties = propOptionNames
            .Select(name => propOptionsType.GetProperty(name, BindingFlags.Public | BindingFlags.Instance))
            .ToArray();

        Assert.IsFalse(propType.IsAbstract);
        Assert.IsFalse(propOptionsType.IsAbstract);
        Assert.IsFalse(propRegistryType.IsAbstract);
        Assert.IsFalse(emitRegistryType.IsAbstract);
        Assert.IsTrue(typeof(VueProps).IsAssignableFrom(propOptionsType));
        Assert.IsTrue(typeof(VueProps).IsAssignableFrom(typeof(VuePropOptions)));
        Assert.IsTrue(typeof(VueProps).IsAssignableFrom(propRegistryType));
        Assert.IsTrue(typeof(VueProps).IsAssignableFrom(typeof(VuePropRegistry)));
        Assert.IsTrue(typeof(VueProps).IsAssignableFrom(emitRegistryType));
        Assert.IsNotNull(propRegistryIndexer);
        Assert.IsNotNull(emitRegistryIndexer);
        CollectionAssert.AreEqual(new[] { typeof(string) }, propRegistryIndexer!.GetIndexParameters().Select(static parameter => parameter.ParameterType).ToArray());
        CollectionAssert.AreEqual(new[] { typeof(string) }, emitRegistryIndexer!.GetIndexParameters().Select(static parameter => parameter.ParameterType).ToArray());
        Assert.AreEqual(typeof(VuePropDeclaration<string>), propRegistryIndexer.PropertyType.UnwrapNullable());
        Assert.AreEqual(typeof(VueEmitValidator<string>), emitRegistryIndexer.PropertyType.UnwrapNullable());
        Assert.IsTrue(propOptionProperties.All(static property => property is not null));

        CollectionAssert.AreEquivalent(
            new[] { "String", "Number", "Boolean", "Array", "Object", "Date", "Function", "Symbol", "Error" },
            propType.GetProperties(BindingFlags.Public | BindingFlags.Static)
                .Select(static property => property.GetCustomAttribute<System.ComponentModel.DescriptionAttribute>()?.Description?.TrimStart('@', '#'))
                .ToArray());

        AssertNotObject(propType, nameof(VuePropType));
        AssertNotObject(propOptionsType, "VuePropOptions<string>");
        AssertNotObject(typeof(VuePropOptions), nameof(VuePropOptions));
        AssertNotObject(propRegistryType, "VuePropRegistry<string>");
        AssertNotObject(typeof(VuePropRegistry), nameof(VuePropRegistry));
        AssertNotObject(emitRegistryType, "VueEmitRegistry<string>");
        AssertNotObject(typeof(VueEmitValidator<string>), "VueEmitValidator<string>");
    }

    [TestMethod]
    public void Vue_DictionaryAndValueHelpers_DoNotExposeObject()
    {
        var vueValueImplicitConversions = typeof(VueValue)
            .GetMethods(BindingFlags.Public | BindingFlags.Static)
            .Where(static method => method.Name == "op_Implicit" && method.ReturnType == typeof(VueValue))
            .ToArray();

        Assert.IsTrue(vueValueImplicitConversions.Any(static method =>
            method.GetParameters().Select(static parameter => parameter.ParameterType).SequenceEqual(new[] { typeof(Action) })));
        AssertNotObject(typeof(VueDictionary), nameof(VueDictionary));
        AssertNotObject(typeof(VueDictionary<string>), nameof(VueDictionary<string>));
        AssertNotObject(typeof(VueDictionary<VueValue>), "VueDictionary<VueValue>");
        AssertNotObject(typeof(VueValue), nameof(VueValue));
        AssertNotObject(typeof(VueKey), nameof(VueKey));
    }

    [TestMethod]
    public void Vue_ComponentAndDirectiveRegistries_ExposeDirectStringKeyedBags()
    {
        var componentRegistryType = typeof(VueComponentRegistry);
        var directiveRegistryType = typeof(VueDirectiveRegistry);
        var componentIndexer = componentRegistryType.GetProperty("Item", BindingFlags.Public | BindingFlags.Instance);
        var directiveIndexer = directiveRegistryType.GetProperty("Item", BindingFlags.Public | BindingFlags.Instance);
        var directiveAddMethods = directiveRegistryType
            .GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
            .Where(static method => method.Name == "Add")
            .ToArray();
        var vuetifyComponents = typeof(VuetifyOptions).GetProperty(nameof(VuetifyOptions.Components), BindingFlags.Public | BindingFlags.Instance);
        var vuetifyDirectives = typeof(VuetifyOptions).GetProperty(nameof(VuetifyOptions.Directives), BindingFlags.Public | BindingFlags.Instance);

        Assert.IsFalse(componentRegistryType.IsAbstract);
        Assert.IsFalse(directiveRegistryType.IsAbstract);
        Assert.IsTrue(typeof(System.Collections.IEnumerable).IsAssignableFrom(directiveRegistryType));
        Assert.IsNotNull(componentIndexer);
        Assert.IsNotNull(directiveIndexer);
        Assert.AreEqual(typeof(ECMAScript.Vue3.IVueComponent), componentIndexer.PropertyType.UnwrapNullable());
        Assert.AreEqual(typeof(VueDirective), directiveIndexer.PropertyType.UnwrapNullable());
        CollectionAssert.AreEqual(new[] { typeof(string) }, componentIndexer.GetIndexParameters().Select(static parameter => parameter.ParameterType).ToArray());
        CollectionAssert.AreEqual(new[] { typeof(string) }, directiveIndexer.GetIndexParameters().Select(static parameter => parameter.ParameterType).ToArray());
        Assert.AreEqual(4, directiveAddMethods.Length);
        Assert.IsTrue(directiveAddMethods.Any(static method =>
            !method.IsGenericMethodDefinition &&
            method.ReturnType == typeof(void) &&
            method.GetParameters().Select(static parameter => parameter.ParameterType).SequenceEqual(new[] { typeof(string), typeof(VueDirective) })));
        Assert.IsTrue(directiveAddMethods.Any(static method =>
            method.IsGenericMethodDefinition &&
            method.GetGenericArguments().Length == 1 &&
            method.ReturnType == typeof(void) &&
            method.GetParameters().Length == 2 &&
            method.GetParameters()[0].ParameterType == typeof(string) &&
            method.GetParameters()[1].ParameterType.IsGenericType &&
            method.GetParameters()[1].ParameterType.GetGenericTypeDefinition() == typeof(VueDirective<>)));
        Assert.IsTrue(directiveAddMethods.Any(static method =>
            !method.IsGenericMethodDefinition &&
            method.ReturnType == typeof(void) &&
            method.GetParameters().Select(static parameter => parameter.ParameterType).SequenceEqual(new[] { typeof(string), typeof(VueDirectiveFunction) })));
        Assert.IsTrue(directiveAddMethods.Any(static method =>
            method.IsGenericMethodDefinition &&
            method.GetGenericArguments().Length == 1 &&
            method.ReturnType == typeof(void) &&
            method.GetParameters().Length == 2 &&
            method.GetParameters()[0].ParameterType == typeof(string) &&
            method.GetParameters()[1].ParameterType.IsGenericType &&
            method.GetParameters()[1].ParameterType.GetGenericTypeDefinition() == typeof(VueDirectiveFunction<>)));
        Assert.IsNotNull(vuetifyComponents);
        Assert.IsNotNull(vuetifyDirectives);
        Assert.AreEqual(typeof(VueComponentRegistry), vuetifyComponents.PropertyType.UnwrapNullable());
        Assert.AreEqual(typeof(VueDirectiveRegistry), vuetifyDirectives.PropertyType.UnwrapNullable());
        Assert.IsTrue(typeof(VueComponentRegistry).IsAssignableFrom(typeof(VuetifyComponentRegistry)));
        Assert.IsTrue(typeof(VueComponentRegistry).IsAssignableFrom(typeof(VuetifyLabsComponentRegistry)));
        Assert.IsTrue(typeof(VueDirectiveRegistry).IsAssignableFrom(typeof(VuetifyDirectiveRegistry)));
    }

    [TestMethod]
    public void Vue_Directives_ExposeDirectObjectAndTypedBindingSurfaces()
    {
        var directiveType = typeof(VueDirective);
        var typedDirectiveType = typeof(VueDirective<string>);
        var mounted = directiveType.GetProperty(nameof(VueDirective.Mounted), BindingFlags.Public | BindingFlags.Instance);
        var typedMounted = typedDirectiveType
            .GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Single(static property =>
                property.Name == nameof(VueDirective<string>.Mounted) &&
                property.PropertyType.UnwrapNullable() == typeof(VueDirectiveHook<string>));
        var bindingType = typeof(VueDirectiveBinding);
        var typedBindingType = typeof(VueDirectiveBinding<string>);
        var updateBindingType = typeof(VueDirectiveUpdateBinding);
        var typedUpdateBindingType = typeof(VueDirectiveUpdateBinding<string>);
        var modifiersType = typeof(VueDirectiveModifiers);
        var modifierBagType = typeof(VueDirectiveModifierBag);
        var directiveArgumentsType = typeof(VueDirectiveArguments);
        var typedDirectiveArgumentsType = typeof(VueDirectiveArguments<string>);
        var directiveValueType = typeof(VueDirectiveValue);
        var modifiersIndexer = modifiersType
            .GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Single(static property =>
                property.Name == "Item" &&
                property.GetIndexParameters().Select(static parameter => parameter.ParameterType).SequenceEqual(new[] { typeof(string) }));
        var modifierBagIndexer = modifierBagType
            .GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Single(static property =>
                property.Name == "Item" &&
                property.GetIndexParameters().Select(static parameter => parameter.ParameterType).SequenceEqual(new[] { typeof(string) }));
        var hookInvoke = typeof(VueDirectiveHook).GetMethod("Invoke", BindingFlags.Public | BindingFlags.Instance);
        var typedHookInvoke = typeof(VueDirectiveHook<string>).GetMethod("Invoke", BindingFlags.Public | BindingFlags.Instance);
        var functionInvoke = typeof(VueDirectiveFunction).GetMethod("Invoke", BindingFlags.Public | BindingFlags.Instance);
        var typedFunctionInvoke = typeof(VueDirectiveFunction<string>).GetMethod("Invoke", BindingFlags.Public | BindingFlags.Instance);
        var directiveMethods = typeof(VueApp)
            .GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
            .Where(static method => method.Name == nameof(VueApp.Directive))
            .ToArray();
        var staticMethods = typeof(Vue3)
            .GetMethods(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly)
            .ToArray();

        Assert.IsFalse(directiveType.IsAbstract);
        Assert.IsFalse(typeof(VueDirective<>).IsAbstract);
        Assert.IsTrue(typeof(VueDirective).IsAssignableFrom(typedDirectiveType));
        Assert.IsTrue(typeof(VueDictionary<bool>).IsAssignableFrom(modifierBagType));
        Assert.IsTrue(directiveArgumentsType.IsAssignableFrom(typedDirectiveArgumentsType));
        Assert.IsNotNull(mounted);
        Assert.IsNotNull(typedMounted);
        Assert.AreEqual(typeof(VueDirectiveHook), mounted.PropertyType.UnwrapNullable());
        Assert.AreEqual(typeof(VueDirectiveHook<string>), typedMounted.PropertyType.UnwrapNullable());
        Assert.IsNotNull(hookInvoke);
        Assert.IsNotNull(typedHookInvoke);
        Assert.IsNotNull(functionInvoke);
        Assert.IsNotNull(typedFunctionInvoke);
        CollectionAssert.AreEqual(
            new[] { typeof(Element), typeof(VueDirectiveBinding), typeof(IVNode) },
            hookInvoke.GetParameters().Select(static parameter => parameter.ParameterType).ToArray());
        CollectionAssert.AreEqual(
            new[] { typeof(Element), typeof(VueDirectiveBinding<string>), typeof(IVNode) },
            typedHookInvoke.GetParameters().Select(static parameter => parameter.ParameterType).ToArray());
        CollectionAssert.AreEqual(
            new[] { typeof(Element), typeof(VueDirectiveBinding) },
            functionInvoke.GetParameters().Select(static parameter => parameter.ParameterType).ToArray());
        CollectionAssert.AreEqual(
            new[] { typeof(Element), typeof(VueDirectiveBinding<string>) },
            typedFunctionInvoke.GetParameters().Select(static parameter => parameter.ParameterType).ToArray());

        Assert.AreEqual(typeof(VueValue), bindingType.GetProperty(nameof(VueDirectiveBinding.Value), BindingFlags.Public | BindingFlags.Instance)!.PropertyType.UnwrapNullable());
        Assert.AreEqual(typeof(string), typedBindingType
            .GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Single(static property =>
                property.Name == nameof(VueDirectiveBinding<string>.Value) &&
                property.PropertyType.UnwrapNullable() == typeof(string))
            .PropertyType.UnwrapNullable());
        Assert.AreEqual(typeof(VueValue), updateBindingType.GetProperty(nameof(VueDirectiveUpdateBinding.OldValue), BindingFlags.Public | BindingFlags.Instance)!.PropertyType.UnwrapNullable());
        Assert.AreEqual(typeof(string), typedUpdateBindingType
            .GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Single(static property =>
                property.Name == nameof(VueDirectiveUpdateBinding<string>.OldValue) &&
                property.PropertyType.UnwrapNullable() == typeof(string))
            .PropertyType.UnwrapNullable());
        Assert.IsNotNull(modifiersIndexer);
        Assert.AreEqual(typeof(bool), modifiersIndexer.PropertyType.UnwrapNullable());
        CollectionAssert.AreEqual(new[] { typeof(string) }, modifiersIndexer.GetIndexParameters().Select(static parameter => parameter.ParameterType).ToArray());
        Assert.IsNotNull(modifierBagIndexer);
        Assert.AreEqual(typeof(bool), modifierBagIndexer.PropertyType.UnwrapNullable());
        CollectionAssert.AreEqual(new[] { typeof(string) }, modifierBagIndexer.GetIndexParameters().Select(static parameter => parameter.ParameterType).ToArray());

        Assert.IsTrue(directiveMethods.Any(static method =>
            !method.IsGenericMethodDefinition &&
            method.GetParameters().Select(static parameter => parameter.ParameterType).SequenceEqual(new[] { typeof(string), typeof(VueDirective) })));
        Assert.IsTrue(directiveMethods.Any(static method =>
            method.IsGenericMethodDefinition &&
            method.GetGenericArguments().Length == 1 &&
            method.GetParameters().Length == 2 &&
            method.GetParameters()[0].ParameterType == typeof(string) &&
            method.GetParameters()[1].ParameterType.IsGenericType &&
            method.GetParameters()[1].ParameterType.GetGenericTypeDefinition() == typeof(VueDirective<>)));
        Assert.IsTrue(directiveMethods.Any(static method =>
            !method.IsGenericMethodDefinition &&
            method.GetParameters().Select(static parameter => parameter.ParameterType).SequenceEqual(new[] { typeof(string), typeof(VueDirectiveFunction) })));
        Assert.IsTrue(directiveMethods.Any(static method =>
            method.IsGenericMethodDefinition &&
            method.GetGenericArguments().Length == 1 &&
            method.GetParameters().Length == 2 &&
            method.GetParameters()[0].ParameterType == typeof(string) &&
            method.GetParameters()[1].ParameterType.IsGenericType &&
            method.GetParameters()[1].ParameterType.GetGenericTypeDefinition() == typeof(VueDirectiveFunction<>)));
        Assert.IsTrue(directiveMethods.Any(static method =>
            !method.IsGenericMethodDefinition &&
            method.GetParameters().Length == 1 &&
            method.GetParameters()[0].ParameterType == typeof(string) &&
            method.ReturnType == typeof(VueDirectiveValue)));
        var withDirectivesMethod = staticMethods.FirstOrDefault(static method =>
            method.Name == nameof(Vue3.WithDirectives) &&
            method.ReturnType == typeof(IVNode) &&
            method.GetParameters().Select(static parameter => parameter.ParameterType).SequenceEqual(new[] { typeof(IVNode), typeof(VueDirectiveArguments[]) }));
        Assert.IsNotNull(withDirectivesMethod);
        Assert.IsTrue(withDirectivesMethod!.GetParameters()[1].IsDefined(typeof(ParamArrayAttribute), inherit: false));
        Assert.IsTrue(withDirectivesMethod.GetParameters()[1].IsDefined(typeof(PreserveParamsArrayAttribute), inherit: false));
        var withModifiersMethod = staticMethods.FirstOrDefault(static method =>
            method.Name == nameof(Vue3.WithModifiers) &&
            !method.IsGenericMethodDefinition &&
            method.ReturnType == typeof(Action) &&
            method.GetParameters().Select(static parameter => parameter.ParameterType).SequenceEqual(new[] { typeof(Action), typeof(string[]) }));
        var withModifiersTypedMethod = staticMethods.FirstOrDefault(static method =>
            method.Name == nameof(Vue3.WithModifiers) &&
            method.IsGenericMethodDefinition &&
            method.ReturnType.IsGenericType &&
            method.ReturnType.GetGenericTypeDefinition() == typeof(VueEventHandler<>) &&
            method.GetParameters()[0].ParameterType.IsGenericType &&
            method.GetParameters()[0].ParameterType.GetGenericTypeDefinition() == typeof(VueEventHandler<>) &&
            method.GetParameters()[1].ParameterType == typeof(string[]));
        Assert.IsNotNull(withModifiersMethod);
        Assert.IsNotNull(withModifiersTypedMethod);
        Assert.IsTrue(withModifiersMethod!.GetParameters()[1].IsDefined(typeof(ParamArrayAttribute), inherit: false));
        Assert.IsTrue(withModifiersMethod.GetParameters()[1].IsDefined(typeof(PreserveParamsArrayAttribute), inherit: false));
        Assert.IsTrue(withModifiersTypedMethod!.GetParameters()[1].IsDefined(typeof(ParamArrayAttribute), inherit: false));
        Assert.IsTrue(withModifiersTypedMethod.GetParameters()[1].IsDefined(typeof(PreserveParamsArrayAttribute), inherit: false));
        Assert.IsTrue(directiveArgumentsType
            .GetConstructors(BindingFlags.Public | BindingFlags.Instance)
            .Any(static constructor => constructor.GetParameters().Select(static parameter => parameter.ParameterType).SequenceEqual(new[] { typeof(VueDirectiveValue), typeof(VueValue), typeof(string), typeof(VueDirectiveModifierBag) })));
        Assert.IsTrue(typedDirectiveArgumentsType
            .GetConstructors(BindingFlags.Public | BindingFlags.Instance)
            .Any(static constructor => constructor.GetParameters().Length == 4 &&
                                      constructor.GetParameters()[0].ParameterType == typeof(VueDirective<string>) &&
                                      constructor.GetParameters()[1].ParameterType == typeof(string) &&
                                      constructor.GetParameters()[2].ParameterType == typeof(string) &&
                                      constructor.GetParameters()[3].ParameterType == typeof(VueDirectiveModifierBag)));

        AssertNotObject(typeof(VueDirective), nameof(VueDirective));
        AssertNotObject(typeof(VueDirective<string>), "VueDirective<string>");
        AssertNotObject(directiveValueType, nameof(VueDirectiveValue));
        AssertNotObject(bindingType, nameof(VueDirectiveBinding));
        AssertNotObject(typedBindingType, "VueDirectiveBinding<string>");
        AssertNotObject(updateBindingType, nameof(VueDirectiveUpdateBinding));
        AssertNotObject(typedUpdateBindingType, "VueDirectiveUpdateBinding<string>");
        AssertNotObject(modifiersType, nameof(VueDirectiveModifiers));
        AssertNotObject(modifierBagType, nameof(VueDirectiveModifierBag));
        AssertNotObject(directiveArgumentsType, nameof(VueDirectiveArguments));
        AssertNotObject(typedDirectiveArgumentsType, "VueDirectiveArguments<string>");
        Assert.IsTrue(typeof(VueDirective).IsAssignableFrom(typeof(VuetifyDirective)));
    }

    [TestMethod]
    public void Vue_PluginOptions_ExposeDirectStringKeyedBag()
    {
        var pluginOptionsType = typeof(VuePluginOptions);
        var pluginOptionsIndexer = pluginOptionsType.GetProperty("Item", BindingFlags.Public | BindingFlags.Instance);

        Assert.IsFalse(pluginOptionsType.IsAbstract);
        Assert.IsNotNull(pluginOptionsIndexer);
        Assert.AreEqual(typeof(VueValue), pluginOptionsIndexer.PropertyType.UnwrapNullable());
        CollectionAssert.AreEqual(new[] { typeof(string) }, pluginOptionsIndexer.GetIndexParameters().Select(static parameter => parameter.ParameterType).ToArray());
        Assert.IsTrue(typeof(VuePluginOptions).IsAssignableFrom(typeof(VuetifyOptions)));
    }

    [TestMethod]
    public void Vue_Plugins_ExposeDirectObjectAndFunctionAuthoringSurfaces()
    {
        var pluginType = typeof(VuePlugin);
        var typedPluginType = typeof(VuePlugin<TestVuePluginOptions>);
        var install = pluginType.GetProperty(nameof(VuePlugin.Install), BindingFlags.Public | BindingFlags.Instance);
        var typedInstall = typedPluginType.GetProperty(nameof(VuePlugin<TestVuePluginOptions>.Install), BindingFlags.Public | BindingFlags.Instance);
        var installCallback = typeof(VuePluginInstallCallback).GetMethod("Invoke", BindingFlags.Public | BindingFlags.Instance);
        var typedInstallCallback = typeof(VuePluginInstallCallback<TestVuePluginOptions>).GetMethod("Invoke", BindingFlags.Public | BindingFlags.Instance);
        var useOverloads = typeof(VueApp)
            .GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
            .Where(static method => method.Name == nameof(VueApp.Use))
            .ToArray();

        Assert.IsFalse(pluginType.IsAbstract);
        Assert.IsFalse(typeof(VuePlugin<>).IsAbstract);
        Assert.IsNotNull(install);
        Assert.IsNotNull(typedInstall);
        Assert.AreEqual(typeof(VuePluginInstallCallback), install.PropertyType.UnwrapNullable());
        Assert.AreEqual(typeof(VuePluginInstallCallback<TestVuePluginOptions>), typedInstall.PropertyType.UnwrapNullable());
        Assert.IsNotNull(installCallback);
        Assert.IsNotNull(typedInstallCallback);
        CollectionAssert.AreEqual(
            new[] { typeof(VueApp) },
            installCallback.GetParameters().Select(static parameter => parameter.ParameterType).ToArray());
        CollectionAssert.AreEqual(
            new[] { typeof(VueApp), typeof(TestVuePluginOptions) },
            typedInstallCallback.GetParameters().Select(static parameter => parameter.ParameterType).ToArray());

        Assert.IsTrue(useOverloads.Any(static method =>
            !method.IsGenericMethodDefinition &&
            method.GetParameters().Select(static parameter => parameter.ParameterType).SequenceEqual(new[] { typeof(VuePlugin) })));
        Assert.IsTrue(useOverloads.Any(static method =>
            !method.IsGenericMethodDefinition &&
            method.GetParameters().Select(static parameter => parameter.ParameterType).SequenceEqual(new[] { typeof(VuePlugin), typeof(VuePluginOptions) })));
        Assert.IsTrue(useOverloads.Any(static method =>
            !method.IsGenericMethodDefinition &&
            method.GetParameters().Select(static parameter => parameter.ParameterType).SequenceEqual(new[] { typeof(VuePluginInstallCallback) })));
        Assert.IsTrue(useOverloads.Any(static method =>
            !method.IsGenericMethodDefinition &&
            method.GetParameters().Select(static parameter => parameter.ParameterType).SequenceEqual(new[] { typeof(VuePluginInstallCallback), typeof(VuePluginOptions) })));
        Assert.IsTrue(useOverloads.Any(static method =>
            method.IsGenericMethodDefinition &&
            method.GetGenericArguments().Length == 1 &&
            method.GetParameters().Length == 2 &&
            method.GetParameters()[0].ParameterType.IsGenericType &&
            method.GetParameters()[0].ParameterType.GetGenericTypeDefinition() == typeof(VuePlugin<>) &&
            method.GetParameters()[1].ParameterType.IsGenericParameter));
        Assert.IsTrue(useOverloads.Any(static method =>
            method.IsGenericMethodDefinition &&
            method.GetGenericArguments().Length == 1 &&
            method.GetParameters().Length == 2 &&
            method.GetParameters()[0].ParameterType.IsGenericType &&
            method.GetParameters()[0].ParameterType.GetGenericTypeDefinition() == typeof(VuePluginInstallCallback<>) &&
            method.GetParameters()[1].ParameterType.IsGenericParameter));

        AssertNotObject(typeof(VuePlugin), nameof(VuePlugin));
        AssertNotObject(typeof(VuePlugin<TestVuePluginOptions>), "VuePlugin<TestVuePluginOptions>");
        AssertNotObject(typeof(VuePluginInstallCallback), nameof(VuePluginInstallCallback));
        AssertNotObject(typeof(VuePluginInstallCallback<TestVuePluginOptions>), "VuePluginInstallCallback<TestVuePluginOptions>");
        Assert.IsTrue(typeof(VuePlugin).IsAssignableFrom(typeof(VuetifyPlugin)));
    }

    [TestMethod]
    public void Vue_CreateApp_And_CreateSSRApp_ExposeTypedRootPropsOverloads()
    {
        var methods = typeof(Vue3)
            .GetMethods(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly)
            .Where(static method => method.Name is nameof(Vue3.CreateApp) or nameof(Vue3.CreateSSRApp))
            .ToArray();

        static bool HasTypedRootPropsOverload(MethodInfo[] methods, string methodName, int genericArity, Func<ParameterInfo[], bool> predicate)
            => methods.Any(method =>
                method.Name == methodName &&
                method.IsGenericMethodDefinition &&
                method.GetGenericArguments().Length == genericArity &&
                predicate(method.GetParameters()));

        Assert.IsTrue(HasTypedRootPropsOverload(
            methods,
            nameof(Vue3.CreateApp),
            1,
            parameters => parameters.Length == 2 &&
                          parameters[0].ParameterType.IsGenericType &&
                          parameters[0].ParameterType.GetGenericTypeDefinition() == typeof(ECMAScript.Vue3.IVueComponent<>) &&
                          parameters[1].ParameterType.IsGenericParameter));

        Assert.IsTrue(HasTypedRootPropsOverload(
            methods,
            nameof(Vue3.CreateApp),
            1,
            parameters => parameters.Length == 2 &&
                          parameters[0].ParameterType.IsGenericType &&
                          parameters[0].ParameterType.GetGenericTypeDefinition() == typeof(ECMAScript.Vue3.IVueComponent<>) &&
                          parameters[1].ParameterType.IsGenericType &&
                          parameters[1].ParameterType.GetGenericTypeDefinition() == typeof(VueObject<>)));

        Assert.IsTrue(HasTypedRootPropsOverload(
            methods,
            nameof(Vue3.CreateApp),
            2,
            parameters => parameters.Length == 2 &&
                          parameters[0].ParameterType.IsGenericType &&
                          parameters[0].ParameterType.GetGenericTypeDefinition() == typeof(ECMAScript.Vue3.IVueComponent<,>) &&
                          parameters[1].ParameterType.IsGenericParameter));

        Assert.IsTrue(HasTypedRootPropsOverload(
            methods,
            nameof(Vue3.CreateApp),
            2,
            parameters => parameters.Length == 2 &&
                          parameters[0].ParameterType.IsGenericType &&
                          parameters[0].ParameterType.GetGenericTypeDefinition() == typeof(ECMAScript.Vue3.IVueComponent<,>) &&
                          parameters[1].ParameterType.IsGenericType &&
                          parameters[1].ParameterType.GetGenericTypeDefinition() == typeof(VueObject<>)));

        Assert.IsTrue(HasTypedRootPropsOverload(
            methods,
            nameof(Vue3.CreateSSRApp),
            1,
            parameters => parameters.Length == 2 &&
                          parameters[0].ParameterType.IsGenericType &&
                          parameters[0].ParameterType.GetGenericTypeDefinition() == typeof(ECMAScript.Vue3.IVueComponent<>) &&
                          parameters[1].ParameterType.IsGenericParameter));

        Assert.IsTrue(HasTypedRootPropsOverload(
            methods,
            nameof(Vue3.CreateSSRApp),
            1,
            parameters => parameters.Length == 2 &&
                          parameters[0].ParameterType.IsGenericType &&
                          parameters[0].ParameterType.GetGenericTypeDefinition() == typeof(ECMAScript.Vue3.IVueComponent<>) &&
                          parameters[1].ParameterType.IsGenericType &&
                          parameters[1].ParameterType.GetGenericTypeDefinition() == typeof(VueObject<>)));

        Assert.IsTrue(HasTypedRootPropsOverload(
            methods,
            nameof(Vue3.CreateSSRApp),
            2,
            parameters => parameters.Length == 2 &&
                          parameters[0].ParameterType.IsGenericType &&
                          parameters[0].ParameterType.GetGenericTypeDefinition() == typeof(ECMAScript.Vue3.IVueComponent<,>) &&
                          parameters[1].ParameterType.IsGenericParameter));

        Assert.IsTrue(HasTypedRootPropsOverload(
            methods,
            nameof(Vue3.CreateSSRApp),
            2,
            parameters => parameters.Length == 2 &&
                          parameters[0].ParameterType.IsGenericType &&
                          parameters[0].ParameterType.GetGenericTypeDefinition() == typeof(ECMAScript.Vue3.IVueComponent<,>) &&
                          parameters[1].ParameterType.IsGenericType &&
                          parameters[1].ParameterType.GetGenericTypeDefinition() == typeof(VueObject<>)));
    }

    [TestMethod]
    public void Vue_PublicApiNames_Follow_VueJsNameProjection_Policy()
    {
        var staticMethods = typeof(Vue3)
            .GetMethods(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly)
            .Where(static method => !method.IsSpecialName)
            .ToArray();

        Assert.IsTrue(staticMethods.Any(static method => method.Name == nameof(Vue3.CreateSSRApp)));
        Assert.IsFalse(staticMethods.Any(static method => method.Name == "CreateSsrApp"));
        Assert.IsFalse(staticMethods.Any(static method => method.Name == "CreateSSRAppFn"));

        AssertDirectVueName(staticMethods, nameof(Vue3.CreateApp), "createApp");
        AssertDirectVueName(staticMethods, nameof(Vue3.CreateSSRApp), "createSSRApp");
        AssertDirectVueName(staticMethods, nameof(Vue3.DefineComponent), "defineComponent");
        AssertDirectVueName(staticMethods, nameof(Vue3.DefineAsyncComponent), "defineAsyncComponent");
        AssertDirectVueName(staticMethods, nameof(Vue3.ResolveComponent), "resolveComponent");
        AssertDirectVueName(staticMethods, nameof(Vue3.ResolveDirective), "resolveDirective");
        AssertDirectVueName(staticMethods, nameof(Vue3.MergeProps), "mergeProps");
        AssertDirectVueName(staticMethods, nameof(Vue3.CloneVNode), "cloneVNode");
        AssertDirectVueName(staticMethods, nameof(Vue3.WithDirectives), "withDirectives");
        AssertDirectVueName(staticMethods, nameof(Vue3.WithModifiers), "withModifiers");
        AssertDirectVueName(staticMethods, nameof(Vue3.H), "h");
        AssertDirectVueName(staticMethods, nameof(Vue3.Ref), "ref");
        AssertDirectVueName(staticMethods, nameof(Vue3.Reactive), "reactive");
        AssertDirectVueName(staticMethods, nameof(Vue3.Readonly), "readonly");
        AssertDirectVueName(staticMethods, nameof(Vue3.ShallowRef), "shallowRef");
        AssertDirectVueName(staticMethods, nameof(Vue3.ShallowReactive), "shallowReactive");
        AssertDirectVueName(staticMethods, nameof(Vue3.ShallowReadonly), "shallowReadonly");
        AssertDirectVueName(staticMethods, nameof(Vue3.TriggerRef), "triggerRef");
        AssertDirectVueName(staticMethods, nameof(Vue3.CustomRef), "customRef");
        AssertDirectVueName(staticMethods, nameof(Vue3.IsRef), "isRef");
        AssertDirectVueName(staticMethods, nameof(Vue3.Unref), "unref");
        AssertDirectVueName(staticMethods, nameof(Vue3.ToRef), "toRef");
        AssertDirectVueName(staticMethods, nameof(Vue3.ToRefs), "toRefs");
        AssertDirectVueName(staticMethods, nameof(Vue3.ToRaw), "toRaw");
        AssertDirectVueName(staticMethods, nameof(Vue3.ToValue), "toValue");
        AssertDirectVueName(staticMethods, nameof(Vue3.MarkRaw), "markRaw");
        AssertDirectVueName(staticMethods, nameof(Vue3.IsProxy), "isProxy");
        AssertDirectVueName(staticMethods, nameof(Vue3.IsReactive), "isReactive");
        AssertDirectVueName(staticMethods, nameof(Vue3.IsReadonly), "isReadonly");
        AssertDirectVueName(staticMethods, nameof(Vue3.Computed), "computed");
        AssertDirectVueName(staticMethods, nameof(Vue3.Watch), "watch");
        AssertDirectVueName(staticMethods, nameof(Vue3.WatchEffect), "watchEffect");
        AssertDirectVueName(staticMethods, nameof(Vue3.WatchPostEffect), "watchPostEffect");
        AssertDirectVueName(staticMethods, nameof(Vue3.WatchSyncEffect), "watchSyncEffect");
        AssertDirectVueName(staticMethods, nameof(Vue3.OnWatcherCleanup), "onWatcherCleanup");
        AssertDirectVueName(staticMethods, nameof(Vue3.NextTick), "nextTick");
        AssertDirectVueName(staticMethods, nameof(Vue3.UseAttrs), "useAttrs");
        AssertDirectVueName(staticMethods, nameof(Vue3.UseSlots), "useSlots");
        AssertDirectVueName(staticMethods, nameof(Vue3.UseTemplateRef), "useTemplateRef");
        AssertDirectVueName(staticMethods, nameof(Vue3.UseId), "useId");
        AssertDirectVueName(staticMethods, nameof(Vue3.UseModel), "useModel");
        AssertDirectVueName(staticMethods, nameof(Vue3.UseHost), "useHost");
        AssertDirectVueName(staticMethods, nameof(Vue3.UseShadowRoot), "useShadowRoot");
        AssertDirectVueName(staticMethods, nameof(Vue3.Provide), "provide");
        AssertDirectVueName(staticMethods, nameof(Vue3.Inject), "inject");
        AssertDirectVueName(staticMethods, nameof(Vue3.HasInjectionContext), "hasInjectionContext");
        AssertDirectVueName(staticMethods, nameof(Vue3.EffectScope), "effectScope");
        AssertDirectVueName(staticMethods, nameof(Vue3.GetCurrentScope), "getCurrentScope");
        AssertDirectVueName(staticMethods, nameof(Vue3.OnScopeDispose), "onScopeDispose");
        AssertDirectVueName(staticMethods, nameof(Vue3.OnMounted), "onMounted");
        AssertDirectVueName(staticMethods, nameof(Vue3.OnBeforeMount), "onBeforeMount");
        AssertDirectVueName(staticMethods, nameof(Vue3.OnBeforeUnmount), "onBeforeUnmount");
        AssertDirectVueName(staticMethods, nameof(Vue3.OnUnmounted), "onUnmounted");
        AssertDirectVueName(staticMethods, nameof(Vue3.OnUpdated), "onUpdated");
        AssertDirectVueName(staticMethods, nameof(Vue3.OnBeforeUpdate), "onBeforeUpdate");
        AssertDirectVueName(staticMethods, nameof(Vue3.OnActivated), "onActivated");
        AssertDirectVueName(staticMethods, nameof(Vue3.OnDeactivated), "onDeactivated");
        AssertDirectVueName(staticMethods, nameof(Vue3.OnErrorCaptured), "onErrorCaptured");
        AssertDirectVueName(staticMethods, nameof(Vue3.OnRenderTracked), "onRenderTracked");
        AssertDirectVueName(staticMethods, nameof(Vue3.OnRenderTriggered), "onRenderTriggered");
        AssertDirectVueName(staticMethods, nameof(Vue3.OnServerPrefetch), "onServerPrefetch");

        AssertHelperVueName(staticMethods, nameof(Vue3.BindThis), null);
        AssertHelperVueName(staticMethods, nameof(Vue3.ModelName), null);
        AssertHelperVueName(staticMethods, nameof(Vue3.ModelPropName), null);
        AssertHelperVueName(staticMethods, nameof(Vue3.ModelUpdateEventName), null);

        CollectionAssert.AreEquivalent(
            new[]
            {
                nameof(Vue3.BindThis),
                nameof(Vue3.ModelName),
                nameof(Vue3.ModelPropName),
                nameof(Vue3.ModelUpdateEventName)
            },
            staticMethods
                .Where(static method =>
                {
                    var description = method.GetCustomAttribute<System.ComponentModel.DescriptionAttribute>()?.Description;
                    return description is null;
                })
                .Select(static method => method.Name)
                .Distinct()
                .Where(static name => name is nameof(Vue3.BindThis)
                    or nameof(Vue3.ModelName)
                    or nameof(Vue3.ModelPropName)
                    or nameof(Vue3.ModelUpdateEventName))
                .ToArray());

        static void AssertDirectVueName(MethodInfo[] methods, string methodName, string runtimeName)
        {
            var matchingMethods = methods.Where(method => method.Name == methodName).ToArray();
            Assert.IsTrue(matchingMethods.Length > 0, $"Missing Vue3.{methodName}.");
            Assert.IsTrue(
                matchingMethods.All(method => method.GetCustomAttribute<System.ComponentModel.DescriptionAttribute>()?.Description == "@#" + runtimeName),
                $"Vue3.{methodName} must map directly to Vue runtime name '{runtimeName}'.");
        }

        static void AssertHelperVueName(MethodInfo[] methods, string methodName, string? runtimeName)
        {
            var matchingMethods = methods.Where(method => method.Name == methodName).ToArray();
            Assert.IsTrue(matchingMethods.Length > 0, $"Missing Vue helper {methodName}.");

            if (runtimeName is null)
            {
                Assert.IsTrue(
                    matchingMethods.All(method => method.GetCustomAttribute<System.ComponentModel.DescriptionAttribute>() is null),
                    $"Vue helper {methodName} should not pretend to be a direct Vue runtime API.");
                return;
            }

            Assert.IsTrue(
                matchingMethods.All(method => method.GetCustomAttribute<System.ComponentModel.DescriptionAttribute>()?.Description == "@#" + runtimeName),
                $"Vue helper alias {methodName} must preserve runtime mapping '{runtimeName}'.");
        }
    }

    [TestMethod]
    public void Vue_DirectiveSSRBindings_Follow_CanonicalProjectionPolicy()
    {
        var directiveType = typeof(VueDirective);
        var typedDirectiveType = typeof(VueDirective<string>);
        var getSSRProps = directiveType.GetProperty(nameof(VueDirective.GetSSRProps), BindingFlags.Public | BindingFlags.Instance);
        var typedGetSSRProps = typedDirectiveType
            .GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Single(static property =>
                property.Name == nameof(VueDirective<string>.GetSSRProps) &&
                property.PropertyType.UnwrapNullable() == typeof(VueDirectiveSSRPropsCallback<string>));

        Assert.IsNotNull(getSSRProps);
        Assert.IsNotNull(typedGetSSRProps);

        Assert.AreEqual(typeof(VueDirectiveSSRPropsCallback), getSSRProps!.PropertyType.UnwrapNullable());
        Assert.AreEqual(typeof(VueDirectiveSSRPropsCallback<string>), typedGetSSRProps!.PropertyType.UnwrapNullable());

        Assert.AreEqual("@#getSSRProps", getSSRProps.GetCustomAttribute<System.ComponentModel.DescriptionAttribute>()?.Description);
        Assert.AreEqual("@#getSSRProps", typedGetSSRProps.GetCustomAttribute<System.ComponentModel.DescriptionAttribute>()?.Description);
        Assert.IsNull(directiveType.GetProperty("GetSsrProps", BindingFlags.Public | BindingFlags.Instance));
        Assert.IsNull(typedDirectiveType.GetProperty("GetSsrProps", BindingFlags.Public | BindingFlags.Instance));
        Assert.IsNull(typeof(Vue3).Assembly.GetType("ECMAScript.VueDirectiveSsrPropsCallback"));
        Assert.IsNull(typeof(Vue3).Assembly.GetType("ECMAScript.VueDirectiveSsrPropsCallback`1"));
    }

    [TestMethod]
    public void Vue_P0CoverageBindings_ExposeStronglyTypedRuntimeSurface()
    {
        var version = typeof(Vue3).GetProperty(nameof(Vue3.Version), BindingFlags.Public | BindingFlags.Static);
        var appVersion = typeof(VueApp).GetProperty(nameof(VueApp.Version), BindingFlags.Public | BindingFlags.Instance);
        var appOnUnmount = typeof(VueApp).GetMethod(nameof(VueApp.OnUnmount), BindingFlags.Public | BindingFlags.Instance);
        var appMixin = typeof(VueApp).GetMethod(nameof(VueApp.Mixin), BindingFlags.Public | BindingFlags.Instance);
        var appRunWithContext = typeof(VueApp)
            .GetMethods(BindingFlags.Public | BindingFlags.Instance)
            .Single(static method => method.Name == nameof(VueApp.RunWithContext));
        var staticMethods = typeof(Vue3)
            .GetMethods(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly)
            .ToArray();

        static MethodInfo RequiredStatic(MethodInfo[] methods, string name, Func<MethodInfo, bool> predicate)
            => methods.Single(method => method.Name == name && predicate(method));

        Assert.IsNotNull(version);
        Assert.IsNotNull(appVersion);
        Assert.AreEqual(typeof(string), version.PropertyType);
        Assert.AreEqual(typeof(string), appVersion.PropertyType);
        Assert.IsNotNull(appOnUnmount);
        Assert.IsNotNull(appMixin);
        CollectionAssert.AreEqual(new[] { typeof(Action) }, appOnUnmount.GetParameters().Select(static parameter => parameter.ParameterType).ToArray());
        Assert.AreEqual(typeof(VueApp), appOnUnmount.ReturnType);
        CollectionAssert.AreEqual(new[] { typeof(VueComponentDefinition) }, appMixin.GetParameters().Select(static parameter => parameter.ParameterType).ToArray());
        Assert.AreEqual(typeof(VueApp), appMixin.ReturnType);
        Assert.IsTrue(appRunWithContext.IsGenericMethodDefinition);
        Assert.AreEqual(typeof(Func<>), appRunWithContext.GetParameters()[0].ParameterType.GetGenericTypeDefinition());
        Assert.AreEqual(appRunWithContext.GetGenericArguments()[0], appRunWithContext.ReturnType);

        RequiredStatic(staticMethods, nameof(Vue3.WatchPostEffect), static method =>
            method.ReturnType == typeof(VueWatchHandle) &&
            method.GetParameters().Select(static parameter => parameter.ParameterType).SequenceEqual(new[] { typeof(Action) }));
        RequiredStatic(staticMethods, nameof(Vue3.WatchSyncEffect), static method =>
            method.ReturnType == typeof(VueWatchHandle) &&
            method.GetParameters().Select(static parameter => parameter.ParameterType).SequenceEqual(new[] { typeof(Action) }));
        RequiredStatic(staticMethods, nameof(Vue3.TriggerRef), static method =>
            method.IsGenericMethodDefinition &&
            method.ReturnType == typeof(void) &&
            method.GetParameters()[0].ParameterType.GetGenericTypeDefinition() == typeof(VueShallowRef<>));
        RequiredStatic(staticMethods, nameof(Vue3.ShallowReactive), static method =>
            method.IsGenericMethodDefinition &&
            method.ReturnType.IsGenericParameter);
        RequiredStatic(staticMethods, nameof(Vue3.ShallowReadonly), static method =>
            method.IsGenericMethodDefinition &&
            method.ReturnType.IsGenericParameter);
        RequiredStatic(staticMethods, nameof(Vue3.ToRaw), static method =>
            method.IsGenericMethodDefinition &&
            method.ReturnType.IsGenericParameter);
        RequiredStatic(staticMethods, nameof(Vue3.MarkRaw), static method =>
            method.IsGenericMethodDefinition &&
            method.ReturnType.IsGenericParameter);
        RequiredStatic(staticMethods, nameof(Vue3.IsRef), static method =>
            method.IsGenericMethodDefinition &&
            method.ReturnType == typeof(bool));
        RequiredStatic(staticMethods, nameof(Vue3.Unref), static method =>
            method.IsGenericMethodDefinition &&
            method.GetParameters()[0].ParameterType.IsGenericParameter);
        RequiredStatic(staticMethods, nameof(Vue3.Unref), static method =>
            method.IsGenericMethodDefinition &&
            method.GetParameters()[0].ParameterType.IsGenericType &&
            method.GetParameters()[0].ParameterType.GetGenericTypeDefinition() == typeof(IVueRef<>));
        RequiredStatic(staticMethods, nameof(Vue3.Unref), static method =>
            method.IsGenericMethodDefinition &&
            method.GetParameters()[0].ParameterType.IsGenericType &&
            method.GetParameters()[0].ParameterType.GetGenericTypeDefinition() == typeof(VueShallowRef<>));
        RequiredStatic(staticMethods, nameof(Vue3.NextTick), static method =>
            method.ReturnType == typeof(PromiseResult) &&
            method.GetParameters().Length == 0);
        RequiredStatic(staticMethods, nameof(Vue3.NextTick), static method =>
            method.ReturnType == typeof(PromiseResult) &&
            method.GetParameters().Select(static parameter => parameter.ParameterType).SequenceEqual(new[] { typeof(Action) }));
        RequiredStatic(staticMethods, nameof(Vue3.IsProxy), static method =>
            method.IsGenericMethodDefinition &&
            method.ReturnType == typeof(bool));
        RequiredStatic(staticMethods, nameof(Vue3.IsReactive), static method =>
            method.IsGenericMethodDefinition &&
            method.ReturnType == typeof(bool));
        RequiredStatic(staticMethods, nameof(Vue3.IsReadonly), static method =>
            method.IsGenericMethodDefinition &&
            method.ReturnType == typeof(bool));
        RequiredStatic(staticMethods, nameof(Vue3.HasInjectionContext), static method =>
            method.ReturnType == typeof(bool) &&
            method.GetParameters().Length == 0);
        RequiredStatic(staticMethods, nameof(Vue3.OnBeforeMount), static method =>
            method.ReturnType == typeof(void) &&
            method.GetParameters().Select(static parameter => parameter.ParameterType).SequenceEqual(new[] { typeof(Action) }));
        RequiredStatic(staticMethods, nameof(Vue3.OnBeforeUpdate), static method =>
            method.ReturnType == typeof(void) &&
            method.GetParameters().Select(static parameter => parameter.ParameterType).SequenceEqual(new[] { typeof(Action) }));
        RequiredStatic(staticMethods, nameof(Vue3.OnBeforeUnmount), static method =>
            method.ReturnType == typeof(void) &&
            method.GetParameters().Select(static parameter => parameter.ParameterType).SequenceEqual(new[] { typeof(Action) }));
        RequiredStatic(staticMethods, nameof(Vue3.OnErrorCaptured), static method =>
            method.ReturnType == typeof(void) &&
            method.GetParameters().Select(static parameter => parameter.ParameterType).SequenceEqual(new[] { typeof(VueErrorCapturedHandler) }));
        RequiredStatic(staticMethods, nameof(Vue3.OnErrorCaptured), static method =>
            method.ReturnType == typeof(void) &&
            method.GetParameters().Select(static parameter => parameter.ParameterType).SequenceEqual(new[] { typeof(VueErrorCapturedCallback) }));
        RequiredStatic(staticMethods, nameof(Vue3.OnActivated), static method =>
            method.ReturnType == typeof(void) &&
            method.GetParameters().Select(static parameter => parameter.ParameterType).SequenceEqual(new[] { typeof(Action) }));
        RequiredStatic(staticMethods, nameof(Vue3.OnDeactivated), static method =>
            method.ReturnType == typeof(void) &&
            method.GetParameters().Select(static parameter => parameter.ParameterType).SequenceEqual(new[] { typeof(Action) }));
        RequiredStatic(staticMethods, nameof(Vue3.OnRenderTracked), static method =>
            method.ReturnType == typeof(void) &&
            method.GetParameters().Select(static parameter => parameter.ParameterType).SequenceEqual(new[] { typeof(VueDebuggerCallback) }));
        RequiredStatic(staticMethods, nameof(Vue3.OnRenderTriggered), static method =>
            method.ReturnType == typeof(void) &&
            method.GetParameters().Select(static parameter => parameter.ParameterType).SequenceEqual(new[] { typeof(VueDebuggerCallback) }));
        RequiredStatic(staticMethods, nameof(Vue3.OnServerPrefetch), static method =>
            method.ReturnType == typeof(void) &&
            method.GetParameters().Select(static parameter => parameter.ParameterType).SequenceEqual(new[] { typeof(VueServerPrefetchPromiseCallback) }));
        RequiredStatic(staticMethods, nameof(Vue3.OnServerPrefetch), static method =>
            method.ReturnType == typeof(void) &&
            method.GetParameters().Select(static parameter => parameter.ParameterType).SequenceEqual(new[] { typeof(VueServerPrefetchCallback) }));
        RequiredStatic(staticMethods, nameof(Vue3.MergeProps), static method =>
            method.ReturnType == typeof(VueProps) &&
            method.GetParameters()[0].ParameterType == typeof(VueProps[]));
        RequiredStatic(staticMethods, nameof(Vue3.CloneVNode), static method =>
            method.ReturnType == typeof(IVNode) &&
            method.GetParameters().Select(static parameter => parameter.ParameterType).SequenceEqual(new[] { typeof(IVNode) }));
        RequiredStatic(staticMethods, nameof(Vue3.CloneVNode), static method =>
            method.ReturnType == typeof(IVNode) &&
            method.GetParameters().Select(static parameter => parameter.ParameterType).SequenceEqual(new[] { typeof(IVNode), typeof(VueProps) }));
        RequiredStatic(staticMethods, nameof(Vue3.IsVNode), static method =>
            method.IsGenericMethodDefinition &&
            method.ReturnType == typeof(bool));
        RequiredStatic(staticMethods, nameof(Vue3.ResolveComponent), static method =>
            method.ReturnType == typeof(ECMAScript.Vue3.IVueComponent) &&
            method.GetParameters().Select(static parameter => parameter.ParameterType).SequenceEqual(new[] { typeof(string) }));
        RequiredStatic(staticMethods, nameof(Vue3.ResolveDirective), static method =>
            method.ReturnType == typeof(VueDirectiveValue) &&
            method.GetParameters().Select(static parameter => parameter.ParameterType).SequenceEqual(new[] { typeof(string) }));
    }

    [TestMethod]
    public void Vue_SetupContextReadBags_ExposeIndexedAndConvenienceMembers()
    {
        var attrsType = typeof(VueAttributeBag);
        var slotsType = typeof(VueSlotBag);
        var listenerAttrsType = typeof(VueAttributeListeners);
        var typedListenerAttrsType = typeof(VueAttributeListeners<>).MakeGenericType(typeof(MouseEvent));
        var scopedSlotsType = typeof(VueScopedSlots<>).MakeGenericType(typeof(string));
        var modelRefType = typeof(VueModelRef<>).MakeGenericType(typeof(string));
        var modelNameType = typeof(VueModelName<,>).MakeGenericType(typeof(TestVueProps), typeof(string));
        var modifierBagType = typeof(VueModelModifiers);
        var attrsIndexer = attrsType.GetProperty("Item", BindingFlags.Public | BindingFlags.Instance);
        var slotsIndexer = slotsType.GetProperty("Item", BindingFlags.Public | BindingFlags.Instance);
        var listenerIndexer = listenerAttrsType.GetProperty("Item", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);
        var typedListenerIndexer = typedListenerAttrsType.GetProperty("Item", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);
        var scopedSlotsIndexer = scopedSlotsType.GetProperty("Item", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);
        var modifierIndexer = modifierBagType.GetProperty("Item", BindingFlags.Public | BindingFlags.Instance);
        var modelNameImplicitFromString = modelNameType.GetMethod("op_Implicit", BindingFlags.Public | BindingFlags.Static, null, new[] { typeof(string) }, null);
        var modelNameImplicitToString = modelNameType.GetMethod("op_Implicit", BindingFlags.Public | BindingFlags.Static, null, new[] { modelNameType }, null);
        var attrsClass = attrsType.GetProperty(nameof(VueAttributeBag.Class), BindingFlags.Public | BindingFlags.Instance);
        var attrsStyle = attrsType.GetProperty(nameof(VueAttributeBag.Style), BindingFlags.Public | BindingFlags.Instance);
        var attrsId = attrsType.GetProperty(nameof(VueAttributeBag.Id), BindingFlags.Public | BindingFlags.Instance);
        var attrsTitle = attrsType.GetProperty(nameof(VueAttributeBag.Title), BindingFlags.Public | BindingFlags.Instance);
        var attrsFor = attrsType.GetProperty(nameof(VueAttributeBag.For), BindingFlags.Public | BindingFlags.Instance);
        var attrsName = attrsType.GetProperty(nameof(VueAttributeBag.Name), BindingFlags.Public | BindingFlags.Instance);
        var attrsTypeProp = attrsType.GetProperty(nameof(VueAttributeBag.Type), BindingFlags.Public | BindingFlags.Instance);
        var attrsPlaceholder = attrsType.GetProperty(nameof(VueAttributeBag.Placeholder), BindingFlags.Public | BindingFlags.Instance);
        var attrsDisabled = attrsType.GetProperty(nameof(VueAttributeBag.Disabled), BindingFlags.Public | BindingFlags.Instance);
        var attrsReadOnly = attrsType.GetProperty(nameof(VueAttributeBag.Readonly), BindingFlags.Public | BindingFlags.Instance);
        var attrsRequired = attrsType.GetProperty(nameof(VueAttributeBag.Required), BindingFlags.Public | BindingFlags.Instance);
        var attrsTabIndex = attrsType.GetProperty(nameof(VueAttributeBag.Tabindex), BindingFlags.Public | BindingFlags.Instance);
        var attrsRole = attrsType.GetProperty(nameof(VueAttributeBag.Role), BindingFlags.Public | BindingFlags.Instance);
        var defaultSlot = slotsType.GetProperty(nameof(VueSlotBag.Default), BindingFlags.Public | BindingFlags.Instance);
        var scopedDefaultSlot = scopedSlotsType.GetProperty("Default", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);
        var modelOptionsType = typeof(VueModelOptions<>).MakeGenericType(typeof(string));
        var modelValue = modelRefType.GetProperty(nameof(IVueRef<string>.Value), BindingFlags.Public | BindingFlags.Instance);
        var modelModifiers = modelRefType.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
            .Where(static method => method.Name == nameof(VueModelRef<string>.GetModifiers))
            .ToArray();
        var modifierTrim = modifierBagType.GetProperty(nameof(VueModelModifiers.Trim), BindingFlags.Public | BindingFlags.Instance);
        var modifierNumber = modifierBagType.GetProperty(nameof(VueModelModifiers.Number), BindingFlags.Public | BindingFlags.Instance);
        var modifierLazy = modifierBagType.GetProperty(nameof(VueModelModifiers.Lazy), BindingFlags.Public | BindingFlags.Instance);

        Assert.IsNotNull(attrsIndexer);
        Assert.IsNotNull(slotsIndexer);
        Assert.IsNotNull(listenerIndexer);
        Assert.IsNotNull(typedListenerIndexer);
        Assert.IsNotNull(scopedSlotsIndexer);
        Assert.IsNotNull(modifierIndexer);
        Assert.IsNotNull(modelNameImplicitFromString);
        Assert.IsNotNull(modelNameImplicitToString);
        Assert.IsNotNull(attrsClass);
        Assert.IsNotNull(attrsStyle);
        Assert.IsNotNull(attrsId);
        Assert.IsNotNull(attrsTitle);
        Assert.IsNotNull(attrsFor);
        Assert.IsNotNull(attrsName);
        Assert.IsNotNull(attrsTypeProp);
        Assert.IsNotNull(attrsPlaceholder);
        Assert.IsNotNull(attrsDisabled);
        Assert.IsNotNull(attrsReadOnly);
        Assert.IsNotNull(attrsRequired);
        Assert.IsNotNull(attrsTabIndex);
        Assert.IsNotNull(attrsRole);
        Assert.IsNotNull(defaultSlot);
        Assert.IsNotNull(scopedDefaultSlot);
        Assert.IsNotNull(modelValue);
        Assert.AreEqual(2, modelModifiers.Length);
        Assert.IsNotNull(modifierTrim);
        Assert.IsNotNull(modifierNumber);
        Assert.IsNotNull(modifierLazy);
        Assert.IsFalse(listenerAttrsType.IsAbstract);
        Assert.IsFalse(typedListenerAttrsType.IsAbstract);
        Assert.IsFalse(scopedSlotsType.IsAbstract);
        Assert.IsFalse(modelNameType.IsAbstract);
        Assert.IsTrue(modelRefType.IsAbstract);
        Assert.IsTrue(modifierBagType.IsAbstract);
        Assert.AreEqual(modelNameType, modelNameImplicitFromString.ReturnType);
        Assert.AreEqual(typeof(string), modelNameImplicitToString.ReturnType);
        Assert.IsTrue(typeof(VueProps).IsAssignableFrom(listenerAttrsType));
        Assert.IsTrue(listenerAttrsType.IsAssignableFrom(typedListenerAttrsType));
        Assert.IsTrue(typeof(VueSlots).IsAssignableFrom(scopedSlotsType));
        Assert.AreEqual(typeof(VueValue), attrsIndexer.PropertyType.UnwrapNullable());
        Assert.AreEqual(typeof(VueSlotCallback), slotsIndexer.PropertyType.UnwrapNullable());
        Assert.AreEqual(typeof(Action), listenerIndexer.PropertyType.UnwrapNullable());
        Assert.AreEqual(typeof(VueEventHandler<MouseEvent>), typedListenerIndexer.PropertyType.UnwrapNullable());
        Assert.AreEqual(typeof(VueSlotCallback<string>), scopedSlotsIndexer.PropertyType.UnwrapNullable());
        Assert.AreEqual(typeof(bool), modifierIndexer.PropertyType.UnwrapNullable());
        Assert.AreEqual(typeof(VueClassValue), attrsClass.PropertyType.UnwrapNullable());
        Assert.AreEqual(typeof(VueStyleValue), attrsStyle.PropertyType.UnwrapNullable());
        Assert.AreEqual(typeof(string), attrsId.PropertyType.UnwrapNullable());
        Assert.AreEqual(typeof(string), attrsTitle.PropertyType.UnwrapNullable());
        Assert.AreEqual(typeof(string), attrsFor.PropertyType.UnwrapNullable());
        Assert.AreEqual(typeof(string), attrsName.PropertyType.UnwrapNullable());
        Assert.AreEqual(typeof(string), attrsTypeProp.PropertyType.UnwrapNullable());
        Assert.AreEqual(typeof(string), attrsPlaceholder.PropertyType.UnwrapNullable());
        Assert.AreEqual(typeof(bool), attrsDisabled.PropertyType.UnwrapNullable());
        Assert.AreEqual(typeof(bool), attrsReadOnly.PropertyType.UnwrapNullable());
        Assert.AreEqual(typeof(bool), attrsRequired.PropertyType.UnwrapNullable());
        Assert.AreEqual(typeof(int), attrsTabIndex.PropertyType.UnwrapNullable());
        Assert.AreEqual(typeof(string), attrsRole.PropertyType.UnwrapNullable());
        Assert.IsNull(attrsType.GetProperty("ReadOnly", BindingFlags.Public | BindingFlags.Instance));
        Assert.IsNull(attrsType.GetProperty("TabIndex", BindingFlags.Public | BindingFlags.Instance));
        Assert.AreEqual(typeof(VueSlotCallback), defaultSlot.PropertyType.UnwrapNullable());
        Assert.AreEqual(typeof(VueSlotCallback<string>), scopedDefaultSlot.PropertyType.UnwrapNullable());
        Assert.AreEqual(typeof(Func<string, string>), modelOptionsType.GetProperty(nameof(VueModelOptions<string>.Get))!.PropertyType.UnwrapNullable());
        Assert.AreEqual(typeof(Func<string, string>), modelOptionsType.GetProperty(nameof(VueModelOptions<string>.Set))!.PropertyType.UnwrapNullable());
        Assert.AreEqual(typeof(string), modelValue.PropertyType.UnwrapNullable());
        Assert.AreEqual(typeof(bool), modifierTrim.PropertyType.UnwrapNullable());
        Assert.AreEqual(typeof(bool), modifierNumber.PropertyType.UnwrapNullable());
        Assert.AreEqual(typeof(bool), modifierLazy.PropertyType.UnwrapNullable());
        CollectionAssert.AreEqual(new[] { typeof(string) }, attrsIndexer.GetIndexParameters().Select(static parameter => parameter.ParameterType).ToArray());
        CollectionAssert.AreEqual(new[] { typeof(string) }, slotsIndexer.GetIndexParameters().Select(static parameter => parameter.ParameterType).ToArray());
        CollectionAssert.AreEqual(new[] { typeof(string) }, listenerIndexer.GetIndexParameters().Select(static parameter => parameter.ParameterType).ToArray());
        CollectionAssert.AreEqual(new[] { typeof(string) }, typedListenerIndexer.GetIndexParameters().Select(static parameter => parameter.ParameterType).ToArray());
        CollectionAssert.AreEqual(new[] { typeof(string) }, scopedSlotsIndexer.GetIndexParameters().Select(static parameter => parameter.ParameterType).ToArray());
        CollectionAssert.AreEqual(new[] { typeof(string) }, modifierIndexer.GetIndexParameters().Select(static parameter => parameter.ParameterType).ToArray());

        var staticMethods = typeof(Vue3)
            .GetMethods(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly)
            .ToArray();

        static MethodInfo RequiredStatic(MethodInfo[] methods, string name, Func<MethodInfo, bool> predicate)
            => methods.Single(method => method.Name == name && predicate(method));

        RequiredStatic(staticMethods, nameof(Vue3.UseAttrs), static method =>
            method.ReturnType == typeof(VueAttributeBag) &&
            method.GetParameters().Length == 0);
        RequiredStatic(staticMethods, nameof(Vue3.UseAttrs), static method =>
            method.IsGenericMethodDefinition &&
            method.GetGenericArguments().Length == 1 &&
            method.ReturnType.IsGenericParameter &&
            method.GetParameters().Length == 0);
        RequiredStatic(staticMethods, nameof(Vue3.UseSlots), static method =>
            method.ReturnType == typeof(VueSlotBag) &&
            method.GetParameters().Length == 0);
        RequiredStatic(staticMethods, nameof(Vue3.UseSlots), static method =>
            method.IsGenericMethodDefinition &&
            method.GetGenericArguments().Length == 1 &&
            method.ReturnType.IsGenericParameter &&
            method.GetParameters().Length == 0);
        RequiredStatic(staticMethods, nameof(Vue3.UseTemplateRef), static method =>
            method.IsGenericMethodDefinition &&
            method.ReturnType.IsGenericType &&
            method.ReturnType.GetGenericTypeDefinition() == typeof(VueReadonlyRef<>) &&
            method.GetParameters().Select(static parameter => parameter.ParameterType).SequenceEqual(new[] { typeof(string) }));
        RequiredStatic(staticMethods, nameof(Vue3.UseId), static method =>
            method.ReturnType == typeof(string) &&
            method.GetParameters().Length == 0);
        RequiredStatic(staticMethods, nameof(Vue3.UseModel), static method =>
            method.IsGenericMethodDefinition &&
            method.ReturnType.IsGenericType &&
            method.ReturnType.GetGenericTypeDefinition() == typeof(VueModelRef<>) &&
            method.GetParameters().Select(static parameter => parameter.ParameterType).SequenceEqual(new[] { typeof(VueProps), typeof(string) }));
        RequiredStatic(staticMethods, nameof(Vue3.UseModel), static method =>
            method.IsGenericMethodDefinition &&
            method.ReturnType.IsGenericType &&
            method.ReturnType.GetGenericTypeDefinition() == typeof(VueModelRef<>) &&
            method.GetParameters().Length == 3 &&
            method.GetParameters()[0].ParameterType == typeof(VueProps) &&
            method.GetParameters()[1].ParameterType == typeof(string) &&
            method.GetParameters()[2].ParameterType.IsGenericType &&
            method.GetParameters()[2].ParameterType.GetGenericTypeDefinition() == typeof(VueModelOptions<>));
        RequiredStatic(staticMethods, nameof(Vue3.UseModel), static method =>
            method.IsGenericMethodDefinition &&
            method.GetGenericArguments().Length == 2 &&
            method.ReturnType.IsGenericType &&
            method.ReturnType.GetGenericTypeDefinition() == typeof(VueModelRef<>) &&
            method.GetParameters().Length == 2 &&
            method.GetParameters()[0].ParameterType.IsGenericParameter &&
            method.GetParameters()[1].ParameterType.IsGenericType &&
            method.GetParameters()[1].ParameterType.GetGenericTypeDefinition() == typeof(VueModelName<,>));
        RequiredStatic(staticMethods, nameof(Vue3.UseModel), static method =>
            method.IsGenericMethodDefinition &&
            method.GetGenericArguments().Length == 2 &&
            method.ReturnType.IsGenericType &&
            method.ReturnType.GetGenericTypeDefinition() == typeof(VueModelRef<>) &&
            method.GetParameters().Length == 3 &&
            method.GetParameters()[0].ParameterType.IsGenericParameter &&
            method.GetParameters()[1].ParameterType.IsGenericType &&
            method.GetParameters()[1].ParameterType.GetGenericTypeDefinition() == typeof(VueModelName<,>) &&
            method.GetParameters()[2].ParameterType.IsGenericType &&
            method.GetParameters()[2].ParameterType.GetGenericTypeDefinition() == typeof(VueModelOptions<>));
        RequiredStatic(staticMethods, nameof(Vue3.ModelName), static method =>
            method.IsGenericMethodDefinition &&
            method.GetGenericArguments().Length == 2 &&
            method.ReturnType.IsGenericType &&
            method.ReturnType.GetGenericTypeDefinition() == typeof(VueModelName<,>) &&
            method.GetParameters().Length == 0 &&
            method.GetCustomAttribute<ECMAScriptInlineAttribute>()?.RawFuncCode == "\"modelValue\"");
        RequiredStatic(staticMethods, nameof(Vue3.ModelName), static method =>
            method.IsGenericMethodDefinition &&
            method.GetGenericArguments().Length == 2 &&
            method.ReturnType.IsGenericType &&
            method.ReturnType.GetGenericTypeDefinition() == typeof(VueModelName<,>) &&
            method.GetParameters().Select(static parameter => parameter.ParameterType).SequenceEqual(new[] { typeof(string) }) &&
            method.GetCustomAttribute<ECMAScriptInlineAttribute>()?.RawFuncCode == "__arg1");
        RequiredStatic(staticMethods, nameof(Vue3.ModelPropName), static method =>
            method.IsGenericMethodDefinition &&
            method.GetGenericArguments().Length == 2 &&
            method.ReturnType == typeof(string) &&
            method.GetParameters().Length == 1 &&
            method.GetParameters()[0].ParameterType.IsGenericType &&
            method.GetParameters()[0].ParameterType.GetGenericTypeDefinition() == typeof(VueModelName<,>) &&
            method.GetCustomAttribute<ECMAScriptInlineAttribute>()?.RawFuncCode == "__arg1");
        RequiredStatic(staticMethods, nameof(Vue3.ModelUpdateEventName), static method =>
            method.IsGenericMethodDefinition &&
            method.GetGenericArguments().Length == 2 &&
            method.ReturnType == typeof(string) &&
            method.GetParameters().Length == 1 &&
            method.GetParameters()[0].ParameterType.IsGenericType &&
            method.GetParameters()[0].ParameterType.GetGenericTypeDefinition() == typeof(VueModelName<,>) &&
            method.GetCustomAttribute<ECMAScriptInlineAttribute>()?.RawFuncCode == "`update:${__arg1}`");

        Assert.IsTrue(modelModifiers.Any(static method =>
            !method.IsGenericMethodDefinition &&
            method.ReturnType == typeof(VueModelModifiers) &&
            method.GetParameters().Length == 0 &&
            method.GetCustomAttribute<ECMAScriptInlineAttribute>()?.RawFuncCode == "__arg1[1]"));
        Assert.IsTrue(modelModifiers.Any(static method =>
            method.IsGenericMethodDefinition &&
            method.ReturnType.IsGenericParameter &&
            method.GetParameters().Length == 0 &&
            method.GetGenericArguments().Length == 1 &&
            method.GetGenericArguments()[0].GetGenericParameterConstraints().Single() == typeof(VueModelModifiers) &&
            method.GetCustomAttribute<ECMAScriptInlineAttribute>()?.RawFuncCode == "__arg1[1]"));

        AssertNotObject(typeof(VueModelOptions<string>), "VueModelOptions<string>");
        AssertNotObject(typeof(VueModelRef<string>), "VueModelRef<string>");
        AssertNotObject(modelNameType, "VueModelName<TestVueProps,string>");
        AssertNotObject(typeof(VueModelModifiers), nameof(VueModelModifiers));
        AssertNotObject(typeof(VueAttributeListeners), nameof(VueAttributeListeners));
        AssertNotObject(typeof(VueAttributeListeners<MouseEvent>), "VueAttributeListeners<MouseEvent>");
        AssertNotObject(typeof(VueScopedSlots<string>), "VueScopedSlots<string>");
    }

    [TestMethod]
    public void Vue_CustomElements_ExposeTypedConstructorAndOptionsSurface()
    {
        var customElementOptionsType = typeof(VueCustomElementOptions);
        var mergedOptionsType = typeof(VueCustomElementComponentOptions);
        var mergedTypedPropsOptionsType = typeof(VueCustomElementComponentOptions<>).MakeGenericType(typeof(TestVueProps));
        var mergedTypedPropsSlotsOptionsType = typeof(VueCustomElementComponentOptions<,>).MakeGenericType(typeof(TestVueProps), typeof(TestVueSlots));
        var mergedTypedSlotsOptionsType = typeof(VueCustomElementSlotComponentOptions<>).MakeGenericType(typeof(TestVueSlots));
        var configureAppInvoke = typeof(VueCustomElementConfigureAppCallback).GetMethod("Invoke", BindingFlags.Public | BindingFlags.Instance);
        var staticMethods = typeof(Vue3)
            .GetMethods(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly)
            .ToArray();

        static MethodInfo RequiredStatic(MethodInfo[] methods, string name, Func<MethodInfo, bool> predicate)
            => methods.Single(method => method.Name == name && predicate(method));

        Assert.IsFalse(customElementOptionsType.IsAbstract);
        Assert.IsFalse(mergedOptionsType.IsAbstract);
        Assert.IsFalse(mergedTypedPropsOptionsType.IsAbstract);
        Assert.IsFalse(mergedTypedPropsSlotsOptionsType.IsAbstract);
        Assert.IsFalse(mergedTypedSlotsOptionsType.IsAbstract);
        Assert.IsNotNull(configureAppInvoke);
        Assert.IsTrue(typeof(VueComponentOptions).IsAssignableFrom(mergedOptionsType));
        Assert.IsTrue(typeof(VueComponentOptions<TestVueProps>).IsAssignableFrom(mergedTypedPropsOptionsType));
        Assert.IsTrue(typeof(VueComponentOptions<TestVueProps, TestVueSlots>).IsAssignableFrom(mergedTypedPropsSlotsOptionsType));
        Assert.IsTrue(typeof(VueSlotComponentOptions<TestVueSlots>).IsAssignableFrom(mergedTypedSlotsOptionsType));
        CollectionAssert.AreEqual(new[] { typeof(VueApp) }, configureAppInvoke.GetParameters().Select(static parameter => parameter.ParameterType).ToArray());
        Assert.AreEqual(typeof(void), configureAppInvoke.ReturnType);
        Assert.AreEqual(typeof(string[]), customElementOptionsType.GetProperty(nameof(VueCustomElementOptions.Styles))!.PropertyType.UnwrapNullable());
        Assert.AreEqual(typeof(VueCustomElementConfigureAppCallback), customElementOptionsType.GetProperty(nameof(VueCustomElementOptions.ConfigureApp))!.PropertyType.UnwrapNullable());
        Assert.AreEqual(typeof(bool), customElementOptionsType.GetProperty(nameof(VueCustomElementOptions.ShadowRoot))!.PropertyType.UnwrapNullable());
        Assert.AreEqual(typeof(ShadowRootInit), customElementOptionsType.GetProperty(nameof(VueCustomElementOptions.ShadowRootOptions))!.PropertyType.UnwrapNullable());
        Assert.AreEqual(typeof(string), customElementOptionsType.GetProperty(nameof(VueCustomElementOptions.Nonce))!.PropertyType.UnwrapNullable());
        Assert.AreEqual(typeof(string[]), mergedOptionsType.GetProperty(nameof(VueCustomElementComponentOptions.Styles))!.PropertyType.UnwrapNullable());
        Assert.AreEqual(typeof(VueCustomElementConfigureAppCallback), mergedOptionsType.GetProperty(nameof(VueCustomElementComponentOptions.ConfigureApp))!.PropertyType.UnwrapNullable());
        Assert.AreEqual(typeof(bool), mergedOptionsType.GetProperty(nameof(VueCustomElementComponentOptions.ShadowRoot))!.PropertyType.UnwrapNullable());
        Assert.AreEqual(typeof(ShadowRootInit), mergedOptionsType.GetProperty(nameof(VueCustomElementComponentOptions.ShadowRootOptions))!.PropertyType.UnwrapNullable());
        Assert.AreEqual(typeof(string), mergedOptionsType.GetProperty(nameof(VueCustomElementComponentOptions.Nonce))!.PropertyType.UnwrapNullable());
        Assert.AreEqual(typeof(string[]), mergedTypedPropsOptionsType.GetProperty("Styles")!.PropertyType.UnwrapNullable());
        Assert.AreEqual(typeof(string[]), mergedTypedPropsSlotsOptionsType.GetProperty("Styles")!.PropertyType.UnwrapNullable());
        Assert.AreEqual(typeof(string[]), mergedTypedSlotsOptionsType.GetProperty("Styles")!.PropertyType.UnwrapNullable());

        RequiredStatic(staticMethods, nameof(Vue3.DefineCustomElement), static method =>
            method.ReturnType == typeof(CustomElementConstructor) &&
            method.GetParameters().Select(static parameter => parameter.ParameterType).SequenceEqual(new[] { typeof(VueComponentDefinition) }));
        RequiredStatic(staticMethods, nameof(Vue3.DefineCustomElement), static method =>
            method.ReturnType == typeof(CustomElementConstructor) &&
            method.GetParameters().Select(static parameter => parameter.ParameterType).SequenceEqual(new[] { typeof(VueComponentDefinition), typeof(VueCustomElementOptions) }));
        RequiredStatic(staticMethods, nameof(Vue3.UseHost), static method =>
            method.ReturnType == typeof(HTMLElement) &&
            method.GetParameters().Length == 0);
        RequiredStatic(staticMethods, nameof(Vue3.UseHost), static method =>
            method.IsGenericMethodDefinition &&
            method.GetGenericArguments().Length == 1 &&
            method.ReturnType.IsGenericParameter &&
            method.GetParameters().Length == 0);
        RequiredStatic(staticMethods, nameof(Vue3.UseShadowRoot), static method =>
            method.ReturnType == typeof(ShadowRoot) &&
            method.GetParameters().Length == 0);

        AssertNotObject(typeof(VueCustomElementOptions), nameof(VueCustomElementOptions));
        AssertNotObject(typeof(VueCustomElementConfigureAppCallback), nameof(VueCustomElementConfigureAppCallback));
        AssertNotObject(typeof(VueCustomElementComponentOptions), nameof(VueCustomElementComponentOptions));
        AssertNotObject(typeof(VueCustomElementComponentOptions<TestVueProps>), "VueCustomElementComponentOptions<TestVueProps>");
        AssertNotObject(typeof(VueCustomElementComponentOptions<TestVueProps, TestVueSlots>), "VueCustomElementComponentOptions<TestVueProps, TestVueSlots>");
        AssertNotObject(typeof(VueCustomElementSlotComponentOptions<TestVueSlots>), "VueCustomElementSlotComponentOptions<TestVueSlots>");
    }

    [TestMethod]
    public void Vue_P1ReactivityBindings_ExposeStronglyTypedHelperSurface()
    {
        var handleMethods = typeof(VueWatchHandle)
            .GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
            .Where(static method => !method.IsSpecialName)
            .ToDictionary(static method => method.Name, StringComparer.Ordinal);
        var debuggerEventType = typeof(VueDebuggerEvent);
        var debuggerCallbackInvoke = typeof(VueDebuggerCallback).GetMethod("Invoke", BindingFlags.Public | BindingFlags.Instance);
        var watchOptionsType = typeof(VueWatchOptions);
        var effectOptionsType = typeof(VueWatchEffectOptions);
        var watchSourcesCallbackInvoke = typeof(VueWatchSourcesCallback<>)
            .MakeGenericType(typeof(int))
            .GetMethod("Invoke", BindingFlags.Public | BindingFlags.Instance);
        var watchSourcesCleanupCallbackInvoke = typeof(VueWatchSourcesCleanupCallback<>)
            .MakeGenericType(typeof(int))
            .GetMethod("Invoke", BindingFlags.Public | BindingFlags.Instance);
        var writableComputedType = typeof(VueWritableComputedOptions<>).MakeGenericType(typeof(int));
        var customRefHandlersType = typeof(VueCustomRefHandlers<>).MakeGenericType(typeof(int));
        var customRefFactoryInvoke = typeof(VueCustomRefFactory<>)
            .MakeGenericType(typeof(int))
            .GetMethod("Invoke", BindingFlags.Public | BindingFlags.Instance);
        var refsType = typeof(VueRefs);
        var typedRefsType = typeof(VueRefs<>).MakeGenericType(typeof(TestVueProps));
        var refsIndexer = refsType.GetProperty("Item", BindingFlags.Public | BindingFlags.Instance);
        var staticMethods = typeof(Vue3)
            .GetMethods(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly)
            .ToArray();

        static MethodInfo RequiredStatic(MethodInfo[] methods, string name, Func<MethodInfo, bool> predicate)
            => methods.Single(method => method.Name == name && predicate(method));

        static bool HasReferenceTypeConstraint(Type genericParameter)
            => (genericParameter.GenericParameterAttributes & GenericParameterAttributes.ReferenceTypeConstraint) != 0;

        CollectionAssert.AreEquivalent(new[] { nameof(VueWatchHandle.Stop), nameof(VueWatchHandle.Pause), nameof(VueWatchHandle.Resume) }, handleMethods.Keys.ToArray());
        Assert.IsTrue(handleMethods.Values.All(static method => method.ReturnType == typeof(void) && method.GetParameters().Length == 0));
        Assert.IsTrue(refsType.IsAbstract);
        Assert.IsTrue(typedRefsType.IsAbstract);
        Assert.IsTrue(refsType.IsAssignableFrom(typedRefsType));
        Assert.IsNotNull(refsIndexer);
        Assert.AreEqual(typeof(IVueRef<VueValue>), refsIndexer.PropertyType.UnwrapNullable());
        CollectionAssert.AreEqual(new[] { typeof(string) }, refsIndexer.GetIndexParameters().Select(static parameter => parameter.ParameterType).ToArray());
        Assert.AreEqual(typeof(VueWatchFlush), effectOptionsType.GetProperty(nameof(VueWatchEffectOptions.Flush))!.PropertyType.UnwrapNullable());
        Assert.AreEqual(typeof(VueDebuggerCallback), effectOptionsType.GetProperty(nameof(VueWatchEffectOptions.OnTrack))!.PropertyType.UnwrapNullable());
        Assert.AreEqual(typeof(VueDebuggerCallback), effectOptionsType.GetProperty(nameof(VueWatchEffectOptions.OnTrigger))!.PropertyType.UnwrapNullable());
        Assert.AreEqual(typeof(VueDebuggerEventType), debuggerEventType.GetProperty(nameof(VueDebuggerEvent.Type))!.PropertyType);
        Assert.AreEqual(typeof(VueValue), debuggerEventType.GetProperty(nameof(VueDebuggerEvent.Target))!.PropertyType.UnwrapNullable());
        Assert.AreEqual(typeof(VueValue), debuggerEventType.GetProperty(nameof(VueDebuggerEvent.NewValue))!.PropertyType.UnwrapNullable());
        Assert.IsNotNull(debuggerCallbackInvoke);
        CollectionAssert.AreEqual(new[] { typeof(VueDebuggerEvent) }, debuggerCallbackInvoke.GetParameters().Select(static parameter => parameter.ParameterType).ToArray());
        Assert.AreEqual(typeof(bool), watchOptionsType.GetProperty(nameof(VueWatchOptions.Immediate))!.PropertyType.UnwrapNullable());
        Assert.AreEqual(typeof(VueWatchDeep), watchOptionsType.GetProperty(nameof(VueWatchOptions.Deep))!.PropertyType.UnwrapNullable());
        Assert.AreEqual(typeof(bool), watchOptionsType.GetProperty(nameof(VueWatchOptions.Once))!.PropertyType.UnwrapNullable());
        Assert.IsNotNull(watchSourcesCallbackInvoke);
        Assert.IsNotNull(watchSourcesCleanupCallbackInvoke);
        CollectionAssert.AreEqual(new[] { typeof(int[]), typeof(int[]) }, watchSourcesCallbackInvoke.GetParameters().Select(static parameter => parameter.ParameterType).ToArray());
        CollectionAssert.AreEqual(
            new[] { typeof(int[]), typeof(int[]), typeof(VueWatchCleanupRegistration) },
            watchSourcesCleanupCallbackInvoke.GetParameters().Select(static parameter => parameter.ParameterType).ToArray());
        Assert.AreEqual(typeof(Func<int>), writableComputedType.GetProperty(nameof(VueWritableComputedOptions<int>.Get))!.PropertyType);
        Assert.AreEqual(typeof(Action<int>), writableComputedType.GetProperty(nameof(VueWritableComputedOptions<int>.Set))!.PropertyType);
        Assert.AreEqual(typeof(Func<int>), customRefHandlersType.GetProperty(nameof(VueCustomRefHandlers<int>.Get))!.PropertyType);
        Assert.AreEqual(typeof(Action<int>), customRefHandlersType.GetProperty(nameof(VueCustomRefHandlers<int>.Set))!.PropertyType);
        Assert.IsNotNull(customRefFactoryInvoke);
        Assert.AreEqual(typeof(VueCustomRefHandlers<int>), customRefFactoryInvoke.ReturnType);
        CollectionAssert.AreEqual(new[] { typeof(Action), typeof(Action) }, customRefFactoryInvoke.GetParameters().Select(static parameter => parameter.ParameterType).ToArray());
        CollectionAssert.Contains(
            typeof(VueWatchFlush).GetMember(nameof(VueWatchFlush.Post)).Single().CustomAttributes.Select(static attribute => attribute.AttributeType.Name).ToArray(),
            "DescriptionAttribute");

        RequiredStatic(staticMethods, nameof(Vue3.Computed), static method =>
            method.IsGenericMethodDefinition &&
            method.ReturnType.GetGenericTypeDefinition() == typeof(VueWritableComputedRef<>) &&
            method.GetParameters()[0].ParameterType.GetGenericTypeDefinition() == typeof(VueWritableComputedOptions<>));
        RequiredStatic(staticMethods, nameof(Vue3.Watch), static method =>
            method.IsGenericMethodDefinition &&
            method.ReturnType == typeof(VueWatchHandle) &&
            method.GetParameters().Select(static parameter => parameter.ParameterType).SequenceEqual(new[]
            {
                typeof(Func<>).MakeGenericType(method.GetGenericArguments()[0]),
                typeof(Action<,>).MakeGenericType(method.GetGenericArguments()[0], method.GetGenericArguments()[0]),
                typeof(VueWatchOptions)
            }));
        RequiredStatic(staticMethods, nameof(Vue3.Watch), static method =>
            method.IsGenericMethodDefinition &&
            method.ReturnType == typeof(VueWatchHandle) &&
            method.GetParameters().Length == 2 &&
            method.GetParameters()[0].ParameterType.IsGenericType &&
            method.GetParameters()[0].ParameterType.GetGenericTypeDefinition() == typeof(IVueRef<>) &&
            method.GetParameters()[1].ParameterType.IsGenericType &&
            method.GetParameters()[1].ParameterType.GetGenericTypeDefinition() == typeof(Action<,>));
        RequiredStatic(staticMethods, nameof(Vue3.Watch), static method =>
            method.IsGenericMethodDefinition &&
            method.ReturnType == typeof(VueWatchHandle) &&
            HasReferenceTypeConstraint(method.GetGenericArguments()[0]) &&
            method.GetParameters().Length == 2 &&
            method.GetParameters()[0].ParameterType == method.GetGenericArguments()[0] &&
            method.GetParameters()[1].ParameterType.IsGenericType &&
            method.GetParameters()[1].ParameterType.GetGenericTypeDefinition() == typeof(Action<,>));
        RequiredStatic(staticMethods, nameof(Vue3.Watch), static method =>
            method.IsGenericMethodDefinition &&
            method.ReturnType == typeof(VueWatchHandle) &&
            method.GetParameters().Length == 2 &&
            method.GetParameters()[0].ParameterType.IsGenericType &&
            method.GetParameters()[0].ParameterType.GetGenericTypeDefinition() == typeof(VueReadonlyRef<>) &&
            method.GetParameters()[1].ParameterType.IsGenericType &&
            method.GetParameters()[1].ParameterType.GetGenericTypeDefinition() == typeof(Action<,>));
        RequiredStatic(staticMethods, nameof(Vue3.Watch), static method =>
            method.IsGenericMethodDefinition &&
            method.ReturnType == typeof(VueWatchHandle) &&
            method.GetParameters().Length == 3 &&
            method.GetParameters()[0].ParameterType.IsArray &&
            method.GetParameters()[0].ParameterType.GetElementType()!.IsGenericType &&
            method.GetParameters()[0].ParameterType.GetElementType()!.GetGenericTypeDefinition() == typeof(IVueRef<>) &&
            method.GetParameters()[1].ParameterType.IsGenericType &&
            method.GetParameters()[1].ParameterType.GetGenericTypeDefinition() == typeof(VueWatchSourcesCallback<>) &&
            method.GetParameters()[2].ParameterType == typeof(VueWatchOptions));
        RequiredStatic(staticMethods, nameof(Vue3.Watch), static method =>
            method.IsGenericMethodDefinition &&
            method.ReturnType == typeof(VueWatchHandle) &&
            method.GetParameters().Length == 2 &&
            method.GetParameters()[0].ParameterType.IsArray &&
            method.GetParameters()[0].ParameterType.GetElementType()!.IsGenericType &&
            method.GetParameters()[0].ParameterType.GetElementType()!.GetGenericTypeDefinition() == typeof(Func<>) &&
            method.GetParameters()[1].ParameterType.IsGenericType &&
            method.GetParameters()[1].ParameterType.GetGenericTypeDefinition() == typeof(VueWatchSourcesCleanupCallback<>));
        RequiredStatic(staticMethods, nameof(Vue3.Watch), static method =>
            method.IsGenericMethodDefinition &&
            method.ReturnType == typeof(VueWatchHandle) &&
            method.GetParameters().Length == 2 &&
            method.GetParameters()[0].ParameterType.IsArray &&
            method.GetParameters()[0].ParameterType.GetElementType()!.IsGenericType &&
            method.GetParameters()[0].ParameterType.GetElementType()!.GetGenericTypeDefinition() == typeof(VueReadonlyRef<>) &&
            method.GetParameters()[1].ParameterType.IsGenericType &&
            method.GetParameters()[1].ParameterType.GetGenericTypeDefinition() == typeof(VueWatchSourcesCallback<>));
        RequiredStatic(staticMethods, nameof(Vue3.WatchEffect), static method =>
            method.ReturnType == typeof(VueWatchHandle) &&
            method.GetParameters().Select(static parameter => parameter.ParameterType).SequenceEqual(new[] { typeof(VueWatchEffectCallback), typeof(VueWatchEffectOptions) }));
        RequiredStatic(staticMethods, nameof(Vue3.CustomRef), static method =>
            method.IsGenericMethodDefinition &&
            method.ReturnType.GetGenericTypeDefinition() == typeof(IVueRef<>) &&
            method.GetParameters()[0].ParameterType.GetGenericTypeDefinition() == typeof(VueCustomRefFactory<>));
        RequiredStatic(staticMethods, nameof(Vue3.OnWatcherCleanup), static method =>
            method.ReturnType == typeof(void) &&
            method.GetParameters().Select(static parameter => parameter.ParameterType).SequenceEqual(new[] { typeof(Action) }));
        RequiredStatic(staticMethods, nameof(Vue3.OnWatcherCleanup), static method =>
            method.ReturnType == typeof(void) &&
            method.GetParameters().Select(static parameter => parameter.ParameterType).SequenceEqual(new[] { typeof(Action), typeof(bool) }));
        RequiredStatic(staticMethods, nameof(Vue3.ToValue), static method =>
            method.IsGenericMethodDefinition &&
            method.GetParameters()[0].ParameterType.IsGenericType &&
            method.GetParameters()[0].ParameterType.GetGenericTypeDefinition() == typeof(IVueRef<>));
        RequiredStatic(staticMethods, nameof(Vue3.ToRef), static method =>
            method.IsGenericMethodDefinition &&
            method.GetGenericArguments().Length == 1 &&
            method.ReturnType.IsGenericType &&
            method.ReturnType.GetGenericTypeDefinition() == typeof(IVueRef<>) &&
            method.GetParameters().Select(static parameter => parameter.ParameterType).SequenceEqual(method.GetGenericArguments()));
        RequiredStatic(staticMethods, nameof(Vue3.ToRef), static method =>
            method.IsGenericMethodDefinition &&
            method.GetGenericArguments().Length == 1 &&
            method.ReturnType.IsGenericType &&
            method.ReturnType.GetGenericTypeDefinition() == typeof(IVueRef<>) &&
            method.GetParameters().Length == 1 &&
            method.GetParameters()[0].ParameterType.IsGenericType &&
            method.GetParameters()[0].ParameterType.GetGenericTypeDefinition() == typeof(IVueRef<>));
        RequiredStatic(staticMethods, nameof(Vue3.ToRef), static method =>
            method.IsGenericMethodDefinition &&
            method.GetGenericArguments().Length == 1 &&
            method.ReturnType.IsGenericType &&
            method.ReturnType.GetGenericTypeDefinition() == typeof(VueComputedRef<>) &&
            method.GetParameters().Length == 1 &&
            method.GetParameters()[0].ParameterType.IsGenericType &&
            method.GetParameters()[0].ParameterType.GetGenericTypeDefinition() == typeof(Func<>));
        RequiredStatic(staticMethods, nameof(Vue3.ToRef), static method =>
            method.IsGenericMethodDefinition &&
            method.GetGenericArguments().Length == 2 &&
            HasReferenceTypeConstraint(method.GetGenericArguments()[0]) &&
            method.ReturnType.IsGenericType &&
            method.ReturnType.GetGenericTypeDefinition() == typeof(IVueRef<>) &&
            method.GetParameters().Select(static parameter => parameter.ParameterType).SequenceEqual(new[]
            {
                method.GetGenericArguments()[0],
                typeof(string)
            }));
        RequiredStatic(staticMethods, nameof(Vue3.ToRef), static method =>
            method.IsGenericMethodDefinition &&
            method.GetGenericArguments().Length == 2 &&
            HasReferenceTypeConstraint(method.GetGenericArguments()[0]) &&
            method.ReturnType.IsGenericType &&
            method.ReturnType.GetGenericTypeDefinition() == typeof(IVueRef<>) &&
            method.GetParameters().Select(static parameter => parameter.ParameterType).SequenceEqual(new[]
            {
                method.GetGenericArguments()[0],
                typeof(string),
                method.GetGenericArguments()[1]
            }));
        RequiredStatic(staticMethods, nameof(Vue3.ToRef), static method =>
            method.IsGenericMethodDefinition &&
            method.GetGenericArguments().Length == 1 &&
            method.ReturnType.IsGenericType &&
            method.ReturnType.GetGenericTypeDefinition() == typeof(IVueRef<>) &&
            method.GetParameters().Length == 2 &&
            method.GetParameters()[0].ParameterType.IsGenericType &&
            method.GetParameters()[0].ParameterType.GetGenericTypeDefinition() == typeof(VueDictionary<>) &&
            method.GetParameters()[1].ParameterType == typeof(string));
        RequiredStatic(staticMethods, nameof(Vue3.ToRefs), static method =>
            method.IsGenericMethodDefinition &&
            method.GetGenericArguments().Length == 1 &&
            HasReferenceTypeConstraint(method.GetGenericArguments()[0]) &&
            method.ReturnType.IsGenericType &&
            method.ReturnType.GetGenericTypeDefinition() == typeof(VueRefs<>) &&
            method.GetParameters().Select(static parameter => parameter.ParameterType).SequenceEqual(method.GetGenericArguments()));
        RequiredStatic(staticMethods, nameof(Vue3.ToRefs), static method =>
            method.IsGenericMethodDefinition &&
            method.GetGenericArguments().Length == 1 &&
            method.ReturnType == method.GetGenericArguments()[0] &&
            method.GetGenericArguments()[0].GetGenericParameterConstraints().SequenceEqual(new[] { typeof(VueRefs) }) &&
            method.GetParameters().Select(static parameter => parameter.ParameterType).SequenceEqual(new[] { typeof(VueProps) }));
        RequiredStatic(staticMethods, nameof(Vue3.ToRefs), static method =>
            method.IsGenericMethodDefinition &&
            method.GetGenericArguments().Length == 2 &&
            HasReferenceTypeConstraint(method.GetGenericArguments()[1]) &&
            method.ReturnType == method.GetGenericArguments()[0] &&
            method.GetParameters().Select(static parameter => parameter.ParameterType).SequenceEqual(new[] { method.GetGenericArguments()[1] }) &&
            method.GetGenericArguments()[0].GetGenericParameterConstraints().Single().IsGenericType &&
            method.GetGenericArguments()[0].GetGenericParameterConstraints().Single().GetGenericTypeDefinition() == typeof(VueRefs<>));
        RequiredStatic(staticMethods, nameof(Vue3.Provide), static method =>
            method.IsGenericMethodDefinition &&
            method.ReturnType == typeof(void) &&
            method.GetParameters()[0].ParameterType == typeof(string));
        RequiredStatic(staticMethods, nameof(Vue3.Inject), static method =>
            method.IsGenericMethodDefinition &&
            method.GetParameters().Select(static parameter => parameter.ParameterType).SequenceEqual(new[] { typeof(string) }));
        RequiredStatic(staticMethods, nameof(Vue3.EffectScope), static method =>
            method.ReturnType == typeof(VueEffectScope) &&
            method.GetParameters().Select(static parameter => parameter.ParameterType).SequenceEqual(new[] { typeof(bool) }));
        RequiredStatic(staticMethods, nameof(Vue3.GetCurrentScope), static method =>
            method.ReturnType == typeof(VueEffectScope) &&
            method.GetParameters().Length == 0);
        RequiredStatic(staticMethods, nameof(Vue3.OnScopeDispose), static method =>
            method.ReturnType == typeof(void) &&
            method.GetParameters().Select(static parameter => parameter.ParameterType).SequenceEqual(new[] { typeof(Action), typeof(bool) }));

        AssertNotObject(debuggerEventType, nameof(VueDebuggerEvent));
        AssertNotObject(typeof(VueDebuggerCallback), nameof(VueDebuggerCallback));
        AssertNotObject(typeof(VueWatchSourcesCallback<int>), "VueWatchSourcesCallback<int>");
        AssertNotObject(typeof(VueWatchSourcesCleanupCallback<int>), "VueWatchSourcesCleanupCallback<int>");
        AssertNotObject(refsType, nameof(VueRefs));
        AssertNotObject(typedRefsType, "VueRefs<TestVueProps>");
        AssertNotObject(typeof(VueErrorCapturedHandler), nameof(VueErrorCapturedHandler));
        AssertNotObject(typeof(VueErrorCapturedCallback), nameof(VueErrorCapturedCallback));
        AssertNotObject(typeof(VueServerPrefetchPromiseCallback), nameof(VueServerPrefetchPromiseCallback));
        AssertNotObject(typeof(VueServerPrefetchCallback), nameof(VueServerPrefetchCallback));
    }

    [TestMethod]
    public void Vue_P1AsyncComponentAndInjectionBindings_ExposeTypedHelperSurface()
    {
        var injectionKeyType = typeof(VueInjectionKey<>).MakeGenericType(typeof(int));
        var asyncOptionsType = typeof(VueAsyncComponentOptions);
        var typedAsyncOptionsType = typeof(VueAsyncComponentOptions<>).MakeGenericType(typeof(ECMAScript.Vue3.IVueComponent<TestVueProps>));
        var asyncLoaderInvoke = typeof(VueAsyncComponentLoader).GetMethod("Invoke", BindingFlags.Public | BindingFlags.Instance);
        var typedAsyncLoaderInvoke = typeof(VueAsyncComponentLoader<>)
            .MakeGenericType(typeof(ECMAScript.Vue3.IVueComponent<TestVueProps>))
            .GetMethod("Invoke", BindingFlags.Public | BindingFlags.Instance);
        var asyncErrorInvoke = typeof(VueAsyncComponentErrorCallback).GetMethod("Invoke", BindingFlags.Public | BindingFlags.Instance);
        var staticMethods = typeof(Vue3)
            .GetMethods(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly)
            .ToArray();
        var appMethods = typeof(VueApp)
            .GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
            .ToArray();

        static MethodInfo RequiredMethod(MethodInfo[] methods, string name, Func<MethodInfo, bool> predicate)
            => methods.Single(method => method.Name == name && predicate(method));

        Assert.IsNotNull(asyncLoaderInvoke);
        Assert.IsNotNull(typedAsyncLoaderInvoke);
        Assert.IsNotNull(asyncErrorInvoke);
        Assert.IsTrue(asyncLoaderInvoke.ReturnType.IsGenericType);
        Assert.AreEqual(typeof(IPromise<>), asyncLoaderInvoke.ReturnType.GetGenericTypeDefinition());
        Assert.AreEqual(typeof(ECMAScript.Vue3.IVueComponent), asyncLoaderInvoke.ReturnType.GetGenericArguments()[0]);
        Assert.IsTrue(typedAsyncLoaderInvoke.ReturnType.IsGenericType);
        Assert.AreEqual(typeof(IPromise<>), typedAsyncLoaderInvoke.ReturnType.GetGenericTypeDefinition());
        Assert.AreEqual(typeof(ECMAScript.Vue3.IVueComponent<TestVueProps>), typedAsyncLoaderInvoke.ReturnType.GetGenericArguments()[0]);
        CollectionAssert.AreEqual(
            new[] { typeof(Error), typeof(VueAsyncComponentRetryCallback), typeof(VueAsyncComponentRetryCallback), typeof(Number) },
            asyncErrorInvoke.GetParameters().Select(static parameter => parameter.ParameterType).ToArray());
        Assert.AreEqual(typeof(VueAsyncComponentLoader), asyncOptionsType.GetProperty(nameof(VueAsyncComponentOptions.Loader))!.PropertyType);
        Assert.AreEqual(typeof(ECMAScript.Vue3.IVueComponent), asyncOptionsType.GetProperty(nameof(VueAsyncComponentOptions.LoadingComponent))!.PropertyType.UnwrapNullable());
        Assert.AreEqual(typeof(Number), asyncOptionsType.GetProperty(nameof(VueAsyncComponentOptions.Delay))!.PropertyType.UnwrapNullable());
        Assert.AreEqual(typeof(Number), asyncOptionsType.GetProperty(nameof(VueAsyncComponentOptions.Timeout))!.PropertyType.UnwrapNullable());
        Assert.AreEqual(typeof(bool), asyncOptionsType.GetProperty(nameof(VueAsyncComponentOptions.Suspensible))!.PropertyType.UnwrapNullable());
        Assert.AreEqual(typeof(VueAsyncComponentErrorCallback), asyncOptionsType.GetProperty(nameof(VueAsyncComponentOptions.OnError))!.PropertyType.UnwrapNullable());
        Assert.AreEqual(
            typeof(VueAsyncComponentLoader<>).MakeGenericType(typeof(ECMAScript.Vue3.IVueComponent<TestVueProps>)),
            typedAsyncOptionsType.GetProperty(nameof(VueAsyncComponentOptions<ECMAScript.Vue3.IVueComponent<TestVueProps>>.Loader))!.PropertyType);
        Assert.IsTrue(injectionKeyType.GetMethods(BindingFlags.Public | BindingFlags.Static).Any(static method =>
            method.Name == "op_Implicit" &&
            method.ReturnType.IsGenericType &&
            method.ReturnType.GetGenericTypeDefinition() == typeof(VueInjectionKey<>) &&
            method.GetParameters().Select(static parameter => parameter.ParameterType).SequenceEqual(new[] { typeof(Symbol) })));
        Assert.IsTrue(injectionKeyType.GetMethods(BindingFlags.Public | BindingFlags.Static).Any(static method =>
            method.Name == "op_Implicit" &&
            method.ReturnType == typeof(Symbol) &&
            method.GetParameters()[0].ParameterType.IsGenericType &&
            method.GetParameters()[0].ParameterType.GetGenericTypeDefinition() == typeof(VueInjectionKey<>)));

        RequiredMethod(staticMethods, nameof(Vue3.DefineAsyncComponent), static method =>
            !method.IsGenericMethodDefinition &&
            method.ReturnType == typeof(ECMAScript.Vue3.IVueComponent) &&
            method.GetParameters().Select(static parameter => parameter.ParameterType).SequenceEqual(new[] { typeof(VueAsyncComponentLoader) }));
        RequiredMethod(staticMethods, nameof(Vue3.DefineAsyncComponent), static method =>
            !method.IsGenericMethodDefinition &&
            method.ReturnType == typeof(ECMAScript.Vue3.IVueComponent) &&
            method.GetParameters().Select(static parameter => parameter.ParameterType).SequenceEqual(new[] { typeof(VueAsyncComponentOptions) }));
        RequiredMethod(staticMethods, nameof(Vue3.DefineAsyncComponent), static method =>
            method.IsGenericMethodDefinition &&
            method.ReturnType.IsGenericParameter &&
            method.GetParameters()[0].ParameterType.IsGenericType &&
            method.GetParameters()[0].ParameterType.GetGenericTypeDefinition() == typeof(VueAsyncComponentOptions<>));
        RequiredMethod(staticMethods, nameof(Vue3.Provide), static method =>
            method.IsGenericMethodDefinition &&
            method.GetParameters()[0].ParameterType.IsGenericType &&
            method.GetParameters()[0].ParameterType.GetGenericTypeDefinition() == typeof(VueInjectionKey<>));
        RequiredMethod(staticMethods, nameof(Vue3.Inject), static method =>
            method.IsGenericMethodDefinition &&
            method.GetParameters().Length == 2 &&
            method.GetParameters()[0].ParameterType.IsGenericType &&
            method.GetParameters()[0].ParameterType.GetGenericTypeDefinition() == typeof(VueInjectionKey<>));
        RequiredMethod(appMethods, nameof(VueApp.Provide), static method =>
            method.IsGenericMethodDefinition &&
            method.GetParameters()[0].ParameterType.IsGenericType &&
            method.GetParameters()[0].ParameterType.GetGenericTypeDefinition() == typeof(VueInjectionKey<>));

        AssertNotObject(injectionKeyType, "VueInjectionKey<int>");
        AssertNotObject(asyncOptionsType, nameof(VueAsyncComponentOptions));
        AssertNotObject(typedAsyncOptionsType, "VueAsyncComponentOptions<IVueComponent<TestVueProps>>");
        AssertNotObject(typeof(VueAsyncComponentErrorCallback), nameof(VueAsyncComponentErrorCallback));
    }

    [TestMethod]
    public void Vue_InjectObjectFormHelpers_ExposeTypedEntryAndRegistrySurface()
    {
        var optionsType = typeof(VueInjectOptions<>).MakeGenericType(typeof(string));
        var entryType = typeof(VueInjectEntry<>).MakeGenericType(typeof(string));
        var registryType = typeof(VueInjectRegistry<>).MakeGenericType(typeof(string));
        var nonGenericRegistryType = typeof(VueInjectRegistry);
        var from = optionsType.GetProperty(nameof(VueInjectOptions<string>.From), BindingFlags.Public | BindingFlags.Instance);
        var @default = optionsType.GetProperty(nameof(VueInjectOptions<string>.Default), BindingFlags.Public | BindingFlags.Instance);
        var defaultFactory = optionsType.GetProperty(nameof(VueInjectOptions<string>.DefaultFactory), BindingFlags.Public | BindingFlags.Instance);
        var indexer = registryType.GetProperty("Item", BindingFlags.Public | BindingFlags.Instance);
        var addMethods = registryType
            .GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
            .Where(static method => method.Name == "Add")
            .ToArray();
        var entryImplicitSources = entryType
            .GetMethods(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly)
            .Where(method => method.Name == "op_Implicit" && method.ReturnType == entryType)
            .Select(static method => method.GetParameters().Single().ParameterType)
            .ToArray();

        Assert.IsNotNull(from);
        Assert.IsNotNull(@default);
        Assert.IsNotNull(defaultFactory);
        Assert.IsNotNull(indexer);
        Assert.IsTrue(typeof(VueProps).IsAssignableFrom(nonGenericRegistryType));
        Assert.AreEqual(typeof(VueInjectFrom<string>), from!.PropertyType.UnwrapNullable());
        Assert.AreEqual(typeof(string), @default!.PropertyType.UnwrapNullable());
        Assert.AreEqual(typeof(Func<string>), defaultFactory!.PropertyType.UnwrapNullable());
        Assert.AreEqual(entryType, indexer!.PropertyType.UnwrapNullable());
        CollectionAssert.AreEqual(new[] { typeof(string) }, indexer.GetIndexParameters().Select(static parameter => parameter.ParameterType).ToArray());
        CollectionAssert.Contains(entryImplicitSources, typeof(string));
        CollectionAssert.Contains(entryImplicitSources, typeof(VueInjectionKey<string>));
        CollectionAssert.Contains(entryImplicitSources, typeof(Symbol));
        CollectionAssert.Contains(entryImplicitSources, optionsType);
        Assert.AreEqual(3, addMethods.Length);
        Assert.IsTrue(addMethods.Any(static method =>
            method.ReturnType == typeof(void) &&
            method.GetParameters().Select(static parameter => parameter.ParameterType).SequenceEqual(new[] { typeof(string), typeof(string) })));
        Assert.IsTrue(addMethods.Any(method =>
            method.ReturnType == typeof(void) &&
            method.GetParameters().Select(static parameter => parameter.ParameterType).SequenceEqual(new[] { typeof(string), optionsType })));
        Assert.IsTrue(addMethods.Any(method =>
            method.ReturnType == typeof(void) &&
            method.GetParameters().Select(static parameter => parameter.ParameterType).SequenceEqual(new[] { typeof(string), entryType })));

        AssertNotObject(optionsType, "VueInjectOptions<string>");
        AssertNotObject(entryType, "VueInjectEntry<string>");
        AssertNotObject(registryType, "VueInjectRegistry<string>");
        AssertNotObject(nonGenericRegistryType, nameof(VueInjectRegistry));
    }

    [TestMethod]
    public void Vue_AuthoringSurface_PrefersDirectTypedAssignments_AndUsesHelpersOnlyForLanguageBoundaries()
    {
        var computedUnionType = typeof(VueComputedValue<int>);
        var watchDeclarationType = typeof(VueWatchDeclaration<int>);
        var componentRegistryIndexer = typeof(VueComponentRegistry).GetProperty("Item", BindingFlags.Public | BindingFlags.Instance);
        var eventHandlersIndexer = typeof(VueEventHandlers).GetProperty("Item", BindingFlags.Public | BindingFlags.Instance);
        var typedEventHandlersIndexer = typeof(VueEventHandlers<>)
            .MakeGenericType(typeof(MouseEvent))
            .GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Single(static property =>
                property.Name == "Item" &&
                property.GetIndexParameters().Select(static parameter => parameter.ParameterType).SequenceEqual(new[] { typeof(string) }) &&
                property.PropertyType.UnwrapNullable() == typeof(VueEventHandler<MouseEvent>));

        CollectionAssert.AreEquivalent(
            new[] { typeof(Func<int>), typeof(VueWritableComputedOptions<int>) },
            GetUnionConstructorBranchTypes(computedUnionType));
        CollectionAssert.AreEquivalent(
            new[]
            {
                typeof(string),
                typeof(Action<int, int>),
                typeof(VueWatchCleanupCallback<int>),
                typeof(VueWatchHandlerOptions<int>),
                typeof(VueWatchCleanupHandlerOptions<int>),
                typeof(VueWatchNamedHandlerOptions),
                typeof(VueWatchEntries<int>)
            },
            GetUnionConstructorBranchTypes(watchDeclarationType));

        Assert.IsNotNull(componentRegistryIndexer);
        Assert.IsNotNull(eventHandlersIndexer);
        Assert.IsNotNull(typedEventHandlersIndexer);
        Assert.AreEqual(typeof(ECMAScript.Vue3.IVueComponent), componentRegistryIndexer!.PropertyType.UnwrapNullable());
        Assert.AreEqual(typeof(Action), eventHandlersIndexer!.PropertyType.UnwrapNullable());
        Assert.AreEqual(typeof(VueEventHandler<MouseEvent>), typedEventHandlersIndexer!.PropertyType.UnwrapNullable());
        CollectionAssert.AreEqual(new[] { typeof(string) }, componentRegistryIndexer.GetIndexParameters().Select(static parameter => parameter.ParameterType).ToArray());
        CollectionAssert.AreEqual(new[] { typeof(string) }, eventHandlersIndexer.GetIndexParameters().Select(static parameter => parameter.ParameterType).ToArray());
        CollectionAssert.AreEqual(new[] { typeof(string) }, typedEventHandlersIndexer.GetIndexParameters().Select(static parameter => parameter.ParameterType).ToArray());
    }

    [TestMethod]
    public void Vue_AppConfig_ExposeTypedConfigurationSurface()
    {
        var appConfigType = typeof(VueAppConfig);
        var compilerOptionsType = typeof(VueAppCompilerOptions);
        var globalPropertiesType = typeof(VueGlobalProperties);
        var optionMergeStrategiesType = typeof(VueOptionMergeStrategies);
        var appConfig = typeof(VueApp).GetProperty(nameof(VueApp.Config), BindingFlags.Public | BindingFlags.Instance);
        var errorHandler = appConfigType.GetProperty(nameof(VueAppConfig.ErrorHandler), BindingFlags.Public | BindingFlags.Instance);
        var warnHandler = appConfigType.GetProperty(nameof(VueAppConfig.WarnHandler), BindingFlags.Public | BindingFlags.Instance);
        var performance = appConfigType.GetProperty(nameof(VueAppConfig.Performance), BindingFlags.Public | BindingFlags.Instance);
        var compilerOptions = appConfigType.GetProperty(nameof(VueAppConfig.CompilerOptions), BindingFlags.Public | BindingFlags.Instance);
        var globalProperties = appConfigType.GetProperty(nameof(VueAppConfig.GlobalProperties), BindingFlags.Public | BindingFlags.Instance);
        var optionMergeStrategies = appConfigType.GetProperty(nameof(VueAppConfig.OptionMergeStrategies), BindingFlags.Public | BindingFlags.Instance);
        var idPrefix = appConfigType.GetProperty(nameof(VueAppConfig.IdPrefix), BindingFlags.Public | BindingFlags.Instance);
        var throwUnhandledErrorInProduction = appConfigType.GetProperty(nameof(VueAppConfig.ThrowUnhandledErrorInProduction), BindingFlags.Public | BindingFlags.Instance);
        var isCustomElement = compilerOptionsType.GetProperty(nameof(VueAppCompilerOptions.IsCustomElement), BindingFlags.Public | BindingFlags.Instance);
        var whitespace = compilerOptionsType.GetProperty(nameof(VueAppCompilerOptions.Whitespace), BindingFlags.Public | BindingFlags.Instance);
        var delimiters = compilerOptionsType.GetProperty(nameof(VueAppCompilerOptions.Delimiters), BindingFlags.Public | BindingFlags.Instance);
        var comments = compilerOptionsType.GetProperty(nameof(VueAppCompilerOptions.Comments), BindingFlags.Public | BindingFlags.Instance);
        var globalIndexer = globalPropertiesType.GetProperty("Item", BindingFlags.Public | BindingFlags.Instance);
        var strategyIndexer = optionMergeStrategiesType.GetProperty("Item", BindingFlags.Public | BindingFlags.Instance);
        var errorInvoke = typeof(VueAppErrorHandler).GetMethod("Invoke", BindingFlags.Public | BindingFlags.Instance);
        var warnInvoke = typeof(VueAppWarnHandler).GetMethod("Invoke", BindingFlags.Public | BindingFlags.Instance);
        var customElementInvoke = typeof(VueIsCustomElementCallback).GetMethod("Invoke", BindingFlags.Public | BindingFlags.Instance);
        var mergeInvoke = typeof(VueOptionMergeFunction).GetMethod("Invoke", BindingFlags.Public | BindingFlags.Instance);

        Assert.IsNotNull(appConfig);
        Assert.IsNotNull(errorHandler);
        Assert.IsNotNull(warnHandler);
        Assert.IsNotNull(performance);
        Assert.IsNotNull(compilerOptions);
        Assert.IsNotNull(globalProperties);
        Assert.IsNotNull(optionMergeStrategies);
        Assert.IsNotNull(idPrefix);
        Assert.IsNotNull(throwUnhandledErrorInProduction);
        Assert.IsNotNull(isCustomElement);
        Assert.IsNotNull(whitespace);
        Assert.IsNotNull(delimiters);
        Assert.IsNotNull(comments);
        Assert.IsNotNull(globalIndexer);
        Assert.IsNotNull(strategyIndexer);
        Assert.IsNotNull(errorInvoke);
        Assert.IsNotNull(warnInvoke);
        Assert.IsNotNull(customElementInvoke);
        Assert.IsNotNull(mergeInvoke);

        Assert.AreEqual(typeof(VueAppConfig), appConfig.PropertyType);
        Assert.AreEqual(typeof(VueAppErrorHandler), errorHandler.PropertyType.UnwrapNullable());
        Assert.AreEqual(typeof(VueAppWarnHandler), warnHandler.PropertyType.UnwrapNullable());
        Assert.AreEqual(typeof(bool), performance.PropertyType);
        Assert.AreEqual(typeof(VueAppCompilerOptions), compilerOptions.PropertyType);
        Assert.AreEqual(typeof(VueGlobalProperties), globalProperties.PropertyType);
        Assert.AreEqual(typeof(VueOptionMergeStrategies), optionMergeStrategies.PropertyType);
        Assert.AreEqual(typeof(string), idPrefix.PropertyType.UnwrapNullable());
        Assert.AreEqual(typeof(bool), throwUnhandledErrorInProduction.PropertyType);
        Assert.AreEqual(typeof(VueIsCustomElementCallback), isCustomElement.PropertyType.UnwrapNullable());
        Assert.AreEqual(typeof(VueCompilerWhitespace), whitespace.PropertyType.UnwrapNullable());
        Assert.AreEqual(typeof(string[]), delimiters.PropertyType.UnwrapNullable());
        Assert.AreEqual(typeof(bool), comments.PropertyType);
        Assert.AreEqual(typeof(VueValue), globalIndexer.PropertyType.UnwrapNullable());
        Assert.AreEqual(typeof(VueOptionMergeFunction), strategyIndexer.PropertyType.UnwrapNullable());
        CollectionAssert.AreEqual(new[] { typeof(string) }, globalIndexer.GetIndexParameters().Select(static parameter => parameter.ParameterType).ToArray());
        CollectionAssert.AreEqual(new[] { typeof(string) }, strategyIndexer.GetIndexParameters().Select(static parameter => parameter.ParameterType).ToArray());
        CollectionAssert.AreEqual(
            new[] { typeof(VueValue), typeof(VueComponentPublicInstance), typeof(string) },
            errorInvoke.GetParameters().Select(static parameter => parameter.ParameterType).ToArray());
        CollectionAssert.AreEqual(
            new[] { typeof(string), typeof(VueComponentPublicInstance), typeof(string) },
            warnInvoke.GetParameters().Select(static parameter => parameter.ParameterType).ToArray());
        CollectionAssert.AreEqual(new[] { typeof(string) }, customElementInvoke.GetParameters().Select(static parameter => parameter.ParameterType).ToArray());
        Assert.AreEqual(typeof(bool), customElementInvoke.ReturnType);
        CollectionAssert.AreEqual(new[] { typeof(VueValue), typeof(VueValue) }, mergeInvoke.GetParameters().Select(static parameter => parameter.ParameterType).ToArray());
        Assert.AreEqual(typeof(VueValue), mergeInvoke.ReturnType);
        CollectionAssert.Contains(appConfig.CustomAttributes.Select(static attribute => attribute.AttributeType.Name).ToArray(), "DescriptionAttribute");
        CollectionAssert.Contains(
            typeof(VueCompilerWhitespace).GetMember(nameof(VueCompilerWhitespace.Preserve)).Single().CustomAttributes.Select(static attribute => attribute.AttributeType.Name).ToArray(),
            "DescriptionAttribute");

        foreach (var property in new[] { appConfigType, compilerOptionsType, globalPropertiesType, optionMergeStrategiesType }
            .SelectMany(static type => type.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)))
        {
            AssertNotObject(property.PropertyType, $"{property.DeclaringType?.Name}.{property.Name}");
        }

        foreach (var method in new[] { errorInvoke, warnInvoke, customElementInvoke, mergeInvoke })
        {
            AssertNotObject(method.ReturnType, $"{method.DeclaringType?.Name}.Invoke return");
            foreach (var parameter in method.GetParameters())
                AssertNotObject(parameter.ParameterType, $"{method.DeclaringType?.Name}.Invoke({parameter.Name})");
        }
    }

    [TestMethod]
    public void Vue_GenericComponentOptions_UseTypedSetupAndExplicitContracts()
    {
        var componentOptions = typeof(VueComponentOptions<>).MakeGenericType(typeof(TestVueProps));
        var setup = componentOptions.GetProperty("Setup", BindingFlags.Public | BindingFlags.Instance);
        var props = componentOptions.GetProperty("Props", BindingFlags.Public | BindingFlags.Instance);
        var emits = componentOptions.GetProperty("Emits", BindingFlags.Public | BindingFlags.Instance);
        Assert.IsNotNull(setup);
        Assert.IsNotNull(props);
        Assert.IsNotNull(emits);
        Assert.AreEqual(typeof(VueTypedSetupCallback<TestVueProps>), setup.PropertyType);
        Assert.AreEqual(typeof(VueNamesOrOptions), props.PropertyType.UnwrapNullable());
        Assert.AreEqual(typeof(VueNamesOrOptions), emits.PropertyType.UnwrapNullable());

        foreach (var optionType in new[]
        {
            typeof(VueComponentOptions),
            componentOptions,
            typeof(VueComponentOptions<,>).MakeGenericType(typeof(TestVueProps), typeof(TestVueSlots)),
            typeof(VueSlotComponentOptions<>).MakeGenericType(typeof(TestVueSlots))
        })
        {
            AssertNoHostInferenceAttribute(optionType, "Props");
            AssertNoHostInferenceAttribute(optionType, "Emits");
            Assert.IsNull(optionType.GetProperty("PropOptions", BindingFlags.Public | BindingFlags.Instance), $"{optionType.FullName}.PropOptions");
            Assert.IsNull(optionType.GetProperty("PropNames", BindingFlags.Public | BindingFlags.Instance), $"{optionType.FullName}.PropNames");
            Assert.IsNull(optionType.GetProperty("EmitOptions", BindingFlags.Public | BindingFlags.Instance), $"{optionType.FullName}.EmitOptions");
            Assert.IsNull(optionType.GetProperty("EmitNames", BindingFlags.Public | BindingFlags.Instance), $"{optionType.FullName}.EmitNames");
        }
    }

    [TestMethod]
    public void Vue_HostInferenceAttributes_RemainCompilerContractOnly()
    {
        var propsUsage = typeof(PropsAttribute).GetCustomAttribute<AttributeUsageAttribute>();
        var emitsUsage = typeof(EmitsAttribute).GetCustomAttribute<AttributeUsageAttribute>();
        var propsDefaults = new PropsAttribute();
        var emitsDefaults = new EmitsAttribute();
        var configuredProps = typeof(TestShiftedContractComponentOptions<,>).GetProperty("Props", BindingFlags.Public | BindingFlags.Instance);
        var configuredEmits = typeof(TestShiftedContractComponentOptions<,>).GetProperty("Emits", BindingFlags.Public | BindingFlags.Instance);

        Assert.IsNotNull(propsUsage);
        Assert.IsNotNull(emitsUsage);
        Assert.IsNotNull(configuredProps);
        Assert.IsNotNull(configuredEmits);
        Assert.AreEqual(AttributeTargets.Property, propsUsage.ValidOn);
        Assert.AreEqual(AttributeTargets.Property, emitsUsage.ValidOn);
        Assert.AreEqual(false, propsUsage.AllowMultiple);
        Assert.AreEqual(false, emitsUsage.AllowMultiple);
        Assert.AreEqual(PropsAttribute.DefaultTypeArgumentIndex, propsDefaults.TypeArgumentIndex);
        Assert.AreEqual(EmitsAttribute.DefaultSourceMemberName, emitsDefaults.SourceMemberName);
        CollectionAssert.Contains(
            configuredProps.CustomAttributes.Select(static attribute => attribute.AttributeType.FullName).ToArray(),
            "ECMAScript.Contract.PropsAttribute");
        CollectionAssert.Contains(
            configuredEmits.CustomAttributes.Select(static attribute => attribute.AttributeType.FullName).ToArray(),
            "ECMAScript.Contract.EmitsAttribute");
    }

    [TestMethod]
    public void Vue_GenericComponentOptionsWithSlots_BindTypedComponentAndContextContracts()
    {
        var componentOptions = typeof(VueComponentOptions<,>).MakeGenericType(typeof(TestVueProps), typeof(TestVueSlots));
        var setup = componentOptions.GetProperty("Setup", BindingFlags.Public | BindingFlags.Instance);
        var setupContext = typeof(VueSetupContext<>).MakeGenericType(typeof(TestVueSlots));
        var slots = setupContext.GetProperty("Slots", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);
        var typedComponent = typeof(ECMAScript.Vue3.IVueComponent<,>).MakeGenericType(typeof(TestVueProps), typeof(TestVueSlots));
        var defineComponentOverload = typeof(Vue3)
            .GetMethods(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly)
            .Single(static method =>
                method.Name == nameof(Vue3.DefineComponent) &&
                method.IsGenericMethodDefinition &&
                method.GetGenericArguments().Length == 2);

        Assert.IsNotNull(setup);
        Assert.IsNotNull(slots);
        Assert.AreEqual(typeof(VueTypedSetupCallback<TestVueProps, TestVueSlots>), setup.PropertyType);
        Assert.AreEqual(typeof(TestVueSlots), slots.PropertyType);
        Assert.IsTrue(typeof(ECMAScript.Vue3.IVueComponent<TestVueProps>).IsAssignableFrom(typedComponent));
        Assert.IsTrue(typeof(ECMAScript.Vue3.IVueComponent).IsAssignableFrom(typedComponent));

        var parameters = defineComponentOverload.GetParameters();
        Assert.AreEqual(1, parameters.Length);
        Assert.AreEqual(typeof(VueComponentOptions<,>), parameters[0].ParameterType.GetGenericTypeDefinition());
        Assert.AreEqual(typeof(ECMAScript.Vue3.IVueComponent<,>), defineComponentOverload.ReturnType.GetGenericTypeDefinition());
    }

    [TestMethod]
    public void Vue_SlotComponentOptions_BindTypedSlotOnlyContracts()
    {
        var componentOptions = typeof(VueSlotComponentOptions<>).MakeGenericType(typeof(TestVueSlots));
        var setup = componentOptions.GetProperty("Setup", BindingFlags.Public | BindingFlags.Instance);
        var slotComponent = typeof(ECMAScript.Vue3.IVueSlotComponent<>).MakeGenericType(typeof(TestVueSlots));
        var defineComponentOverload = typeof(Vue3)
            .GetMethods(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly)
            .Single(static method =>
                method.Name == nameof(Vue3.DefineComponent) &&
                method.IsGenericMethodDefinition &&
                method.GetGenericArguments().Length == 1 &&
                method.GetParameters()[0].ParameterType.IsGenericType &&
                method.GetParameters()[0].ParameterType.GetGenericTypeDefinition() == typeof(VueSlotComponentOptions<>));

        Assert.IsNotNull(setup);
        Assert.AreEqual(typeof(VueTypedSlotSetupCallback<TestVueSlots>), setup.PropertyType);
        Assert.IsTrue(typeof(ECMAScript.Vue3.IVueComponent).IsAssignableFrom(slotComponent));

        var parameters = defineComponentOverload.GetParameters();
        Assert.AreEqual(1, parameters.Length);
        Assert.AreEqual(typeof(VueSlotComponentOptions<>), parameters[0].ParameterType.GetGenericTypeDefinition());
        Assert.AreEqual(typeof(ECMAScript.Vue3.IVueSlotComponent<>), defineComponentOverload.ReturnType.GetGenericTypeDefinition());
    }

    [TestMethod]
    public void Vue_ComponentDefinitionAndBuiltIns_ExposeTypedOptionAndComponentSurfaces()
    {
        var definitionType = typeof(VueComponentDefinition);
        var inheritAttrs = definitionType.GetProperty(nameof(VueComponentDefinition.InheritAttrs), BindingFlags.Public | BindingFlags.Instance);
        var expose = definitionType.GetProperty(nameof(VueComponentDefinition.Expose), BindingFlags.Public | BindingFlags.Instance);
        var transition = typeof(Vue3).GetProperty(nameof(Vue3.Transition), BindingFlags.Public | BindingFlags.Static);
        var transitionGroup = typeof(Vue3).GetProperty(nameof(Vue3.TransitionGroup), BindingFlags.Public | BindingFlags.Static);
        var keepAlive = typeof(Vue3).GetProperty(nameof(Vue3.KeepAlive), BindingFlags.Public | BindingFlags.Static);
        var teleport = typeof(Vue3).GetProperty(nameof(Vue3.Teleport), BindingFlags.Public | BindingFlags.Static);
        var suspense = typeof(Vue3).GetProperty(nameof(Vue3.Suspense), BindingFlags.Public | BindingFlags.Static);
        var transitionProps = typeof(VueTransitionProps);
        var transitionGroupProps = typeof(VueTransitionGroupProps);
        var keepAliveProps = typeof(VueKeepAliveProps);
        var teleportProps = typeof(VueTeleportProps);
        var suspenseProps = typeof(VueSuspenseProps);
        var suspenseSlots = typeof(VueSuspenseSlots);

        Assert.IsNotNull(inheritAttrs);
        Assert.IsNotNull(expose);
        Assert.AreEqual(typeof(bool), inheritAttrs.PropertyType.UnwrapNullable());
        Assert.AreEqual(typeof(string[]), expose.PropertyType.UnwrapNullable());
        CollectionAssert.Contains(inheritAttrs.CustomAttributes.Select(static attribute => attribute.AttributeType.Name).ToArray(), "DescriptionAttribute");
        CollectionAssert.Contains(expose.CustomAttributes.Select(static attribute => attribute.AttributeType.Name).ToArray(), "DescriptionAttribute");

        Assert.IsNotNull(transition);
        Assert.IsNotNull(transitionGroup);
        Assert.IsNotNull(keepAlive);
        Assert.IsNotNull(teleport);
        Assert.IsNotNull(suspense);
        Assert.AreEqual(typeof(ECMAScript.Vue3.IVueComponent<VueTransitionProps>), transition.PropertyType);
        Assert.AreEqual(typeof(ECMAScript.Vue3.IVueComponent<VueTransitionGroupProps>), transitionGroup.PropertyType);
        Assert.AreEqual(typeof(ECMAScript.Vue3.IVueComponent<VueKeepAliveProps>), keepAlive.PropertyType);
        Assert.AreEqual(typeof(ECMAScript.Vue3.IVueComponent<VueTeleportProps>), teleport.PropertyType);
        Assert.AreEqual(typeof(ECMAScript.Vue3.IVueComponent<VueSuspenseProps, VueSuspenseSlots>), suspense.PropertyType);
        Assert.IsTrue(typeof(VueProps).IsAssignableFrom(transitionProps));
        Assert.IsTrue(typeof(VueProps).IsAssignableFrom(transitionGroupProps));
        Assert.IsTrue(typeof(VueProps).IsAssignableFrom(keepAliveProps));
        Assert.IsTrue(typeof(VueProps).IsAssignableFrom(teleportProps));
        Assert.IsTrue(typeof(VueProps).IsAssignableFrom(suspenseProps));
        Assert.IsTrue(typeof(VueSlots).IsAssignableFrom(suspenseSlots));
        Assert.AreEqual(typeof(VueTransitionDurationValue), transitionProps.GetProperty(nameof(VueTransitionProps.Duration))!.PropertyType.UnwrapNullable());
        Assert.AreEqual(typeof(VueTransitionMode), transitionProps.GetProperty(nameof(VueTransitionProps.Mode))!.PropertyType.UnwrapNullable());
        Assert.AreEqual(typeof(VueKeepAliveMatch), keepAliveProps.GetProperty(nameof(VueKeepAliveProps.Include))!.PropertyType.UnwrapNullable());
        Assert.AreEqual(typeof(VueIntStringValue), keepAliveProps.GetProperty(nameof(VueKeepAliveProps.Max))!.PropertyType.UnwrapNullable());
        Assert.AreEqual(typeof(VueTeleportTarget), teleportProps.GetProperty(nameof(VueTeleportProps.To))!.PropertyType.UnwrapNullable());
        Assert.AreEqual(typeof(VueSlotCallback), suspenseSlots.GetProperty(nameof(VueSuspenseSlots.Fallback))!.PropertyType.UnwrapNullable());

        AssertNotObject(transitionProps, nameof(VueTransitionProps));
        AssertNotObject(transitionGroupProps, nameof(VueTransitionGroupProps));
        AssertNotObject(keepAliveProps, nameof(VueKeepAliveProps));
        AssertNotObject(teleportProps, nameof(VueTeleportProps));
        AssertNotObject(suspenseProps, nameof(VueSuspenseProps));
        AssertNotObject(suspenseSlots, nameof(VueSuspenseSlots));
    }

    [TestMethod]
    public void Vue_ComponentDefinitionLifecycle_ExposeOptionsApiHookSurface()
    {
        var definitionType = typeof(VueComponentDefinition);

        static PropertyInfo RequiredProperty(Type type, string name, Type expectedType)
        {
            var property = type.GetProperty(name, BindingFlags.Public | BindingFlags.Instance);
            Assert.IsNotNull(property, name);
            Assert.AreEqual(expectedType, property!.PropertyType.UnwrapNullable(), name);
            CollectionAssert.Contains(
                property.CustomAttributes.Select(static attribute => attribute.AttributeType.Name).ToArray(),
                "DescriptionAttribute");
            return property;
        }

        RequiredProperty(definitionType, nameof(VueComponentDefinition.BeforeCreate), typeof(Action));
        RequiredProperty(definitionType, nameof(VueComponentDefinition.Created), typeof(Action));
        RequiredProperty(definitionType, nameof(VueComponentDefinition.BeforeMount), typeof(Action));
        RequiredProperty(definitionType, nameof(VueComponentDefinition.Mounted), typeof(Action));
        RequiredProperty(definitionType, nameof(VueComponentDefinition.BeforeUpdate), typeof(Action));
        RequiredProperty(definitionType, nameof(VueComponentDefinition.Updated), typeof(Action));
        RequiredProperty(definitionType, nameof(VueComponentDefinition.BeforeUnmount), typeof(Action));
        RequiredProperty(definitionType, nameof(VueComponentDefinition.Unmounted), typeof(Action));
        RequiredProperty(definitionType, nameof(VueComponentDefinition.Activated), typeof(Action));
        RequiredProperty(definitionType, nameof(VueComponentDefinition.Deactivated), typeof(Action));
        RequiredProperty(definitionType, nameof(VueComponentDefinition.ErrorCaptured), typeof(VueErrorCapturedCallback));
        RequiredProperty(definitionType, nameof(VueComponentDefinition.RenderTracked), typeof(VueDebuggerCallback));
        RequiredProperty(definitionType, nameof(VueComponentDefinition.RenderTriggered), typeof(VueDebuggerCallback));
        RequiredProperty(definitionType, nameof(VueComponentDefinition.ServerPrefetch), typeof(VueServerPrefetchPromiseCallback));

        AssertNotObject(definitionType, nameof(VueComponentDefinition));
    }

    [TestMethod]
    public void Vue_ComponentDefinitionComposition_ExposeOptionsApiProvideInjectSurface()
    {
        var definitionType = typeof(VueComponentDefinition);
        var provide = definitionType.GetProperty(nameof(VueComponentDefinition.Provide), BindingFlags.Public | BindingFlags.Instance);
        var provideFactory = definitionType.GetProperty(nameof(VueComponentDefinition.ProvideFactory), BindingFlags.Public | BindingFlags.Instance);
        var inject = definitionType.GetProperty(nameof(VueComponentDefinition.Inject), BindingFlags.Public | BindingFlags.Instance);
        var mixins = definitionType.GetProperty(nameof(VueComponentDefinition.Mixins), BindingFlags.Public | BindingFlags.Instance);
        var extends = definitionType.GetProperty(nameof(VueComponentDefinition.Extends), BindingFlags.Public | BindingFlags.Instance);

        Assert.IsNotNull(provide);
        Assert.IsNotNull(provideFactory);
        Assert.IsNotNull(inject);
        Assert.IsNotNull(mixins);
        Assert.IsNotNull(extends);
        Assert.AreEqual(typeof(VueProps), provide!.PropertyType.UnwrapNullable());
        Assert.AreEqual(typeof(VueDataCallback), provideFactory!.PropertyType.UnwrapNullable());
        Assert.AreEqual(typeof(VueNamesOrOptions), inject!.PropertyType.UnwrapNullable());
        Assert.AreEqual(typeof(VueComponentDefinition[]), mixins!.PropertyType.UnwrapNullable());
        Assert.AreEqual(typeof(VueComponentDefinition), extends!.PropertyType.UnwrapNullable());
        CollectionAssert.Contains(
            provide.CustomAttributes.Select(static attribute => attribute.AttributeType.Name).ToArray(),
            "DescriptionAttribute");
        CollectionAssert.Contains(
            provideFactory.CustomAttributes.Select(static attribute => attribute.AttributeType.Name).ToArray(),
            "DescriptionAttribute");
        CollectionAssert.Contains(
            inject.CustomAttributes.Select(static attribute => attribute.AttributeType.Name).ToArray(),
            "DescriptionAttribute");
        CollectionAssert.Contains(
            mixins.CustomAttributes.Select(static attribute => attribute.AttributeType.Name).ToArray(),
            "DescriptionAttribute");
        CollectionAssert.Contains(
            extends.CustomAttributes.Select(static attribute => attribute.AttributeType.Name).ToArray(),
            "DescriptionAttribute");

        AssertNotObject(provide.PropertyType, "VueComponentDefinition.Provide");
        AssertNotObject(provideFactory.PropertyType, "VueComponentDefinition.ProvideFactory");
        AssertNotObject(inject.PropertyType, "VueComponentDefinition.Inject");
        AssertNotObject(mixins.PropertyType, "VueComponentDefinition.Mixins");
        AssertNotObject(extends.PropertyType, "VueComponentDefinition.Extends");
    }

    [TestMethod]
    public void Vue_ComponentDefinitionState_ExposeOptionsApiDataSurface()
    {
        var definitionType = typeof(VueComponentDefinition);
        var data = definitionType.GetProperty(nameof(VueComponentDefinition.Data), BindingFlags.Public | BindingFlags.Instance);
        var dataInvoke = typeof(VueDataCallback).GetMethod("Invoke", BindingFlags.Public | BindingFlags.Instance);

        Assert.IsNotNull(data);
        Assert.IsNotNull(dataInvoke);
        Assert.AreEqual(typeof(VueDataCallback), data!.PropertyType.UnwrapNullable());
        Assert.AreEqual(typeof(VueProps), dataInvoke!.ReturnType);
        Assert.AreEqual(0, dataInvoke.GetParameters().Length);
        CollectionAssert.Contains(
            data.CustomAttributes.Select(static attribute => attribute.AttributeType.Name).ToArray(),
            "DescriptionAttribute");

        AssertNotObject(typeof(VueDataCallback), nameof(VueDataCallback));
        AssertNotObject(data.PropertyType, "VueComponentDefinition.Data");
    }

    [TestMethod]
    public void Vue_ComponentDefinitionState_ExposeOptionsApiComputedSurface()
    {
        var definitionType = typeof(VueComponentDefinition);
        var computed = definitionType.GetProperty(nameof(VueComponentDefinition.Computed), BindingFlags.Public | BindingFlags.Instance);
        var registryType = typeof(VueComputedRegistry<>).MakeGenericType(typeof(int));
        var indexer = registryType.GetProperty("Item", BindingFlags.Public | BindingFlags.Instance);
        var addMethods = registryType
            .GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
            .Where(static method => method.Name == "Add")
            .ToArray();

        Assert.IsNotNull(computed);
        Assert.IsNotNull(indexer);
        Assert.AreEqual(typeof(VueProps), computed!.PropertyType.UnwrapNullable());
        Assert.AreEqual(typeof(VueComputedValue<int>), indexer!.PropertyType.UnwrapNullable());
        CollectionAssert.AreEqual(new[] { typeof(string) }, indexer.GetIndexParameters().Select(static parameter => parameter.ParameterType).ToArray());
        Assert.AreEqual(2, addMethods.Length);
        Assert.IsTrue(addMethods.Any(static method =>
            method.ReturnType == typeof(void) &&
            method.GetParameters().Select(static parameter => parameter.ParameterType).SequenceEqual(new[] { typeof(string), typeof(Func<int>) })));
        Assert.IsTrue(addMethods.Any(static method =>
            method.ReturnType == typeof(void) &&
            method.GetParameters().Select(static parameter => parameter.ParameterType).SequenceEqual(new[] { typeof(string), typeof(VueWritableComputedOptions<int>) })));
        CollectionAssert.Contains(
            computed.CustomAttributes.Select(static attribute => attribute.AttributeType.Name).ToArray(),
            "DescriptionAttribute");

        AssertNotObject(typeof(VueComputedRegistry<int>), "VueComputedRegistry<int>");
        AssertNotObject(computed.PropertyType, "VueComponentDefinition.Computed");
    }

    [TestMethod]
    public void Vue_ComponentDefinitionState_ExposeOptionsApiMethodsSurface()
    {
        var definitionType = typeof(VueComponentDefinition);
        var methods = definitionType.GetProperty(nameof(VueComponentDefinition.Methods), BindingFlags.Public | BindingFlags.Instance);
        var registryType = typeof(VueMethodRegistry<>).MakeGenericType(typeof(Action));
        var indexer = registryType.GetProperty("Item", BindingFlags.Public | BindingFlags.Instance);
        var addMethods = registryType
            .GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
            .Where(static method => method.Name == "Add")
            .ToArray();

        Assert.IsNotNull(methods);
        Assert.IsNotNull(indexer);
        Assert.AreEqual(typeof(VueProps), methods!.PropertyType.UnwrapNullable());
        Assert.AreEqual(typeof(Action), indexer!.PropertyType.UnwrapNullable());
        CollectionAssert.AreEqual(new[] { typeof(string) }, indexer.GetIndexParameters().Select(static parameter => parameter.ParameterType).ToArray());
        Assert.AreEqual(1, addMethods.Length);
        Assert.AreEqual(typeof(void), addMethods[0].ReturnType);
        CollectionAssert.AreEqual(
            new[] { typeof(string), typeof(Action) },
            addMethods[0].GetParameters().Select(static parameter => parameter.ParameterType).ToArray());
        CollectionAssert.Contains(
            methods.CustomAttributes.Select(static attribute => attribute.AttributeType.Name).ToArray(),
            "DescriptionAttribute");

        AssertNotObject(typeof(VueMethodRegistry<Action>), "VueMethodRegistry<Action>");
        AssertNotObject(methods.PropertyType, "VueComponentDefinition.Methods");
    }

    [TestMethod]
    public void Vue_ComponentDefinitionState_ExposeOptionsApiWatchSurface()
    {
        var definitionType = typeof(VueComponentDefinition);
        var watch = definitionType.GetProperty(nameof(VueComponentDefinition.Watch), BindingFlags.Public | BindingFlags.Instance);
        var registryType = typeof(VueWatchRegistry<>).MakeGenericType(typeof(int));
        var indexer = registryType.GetProperty("Item", BindingFlags.Public | BindingFlags.Instance);
        var addMethods = registryType
            .GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
            .Where(static method => method.Name == "Add")
            .ToArray();
        var watchEntryType = typeof(VueWatchEntry<>).MakeGenericType(typeof(int));
        var watchEntriesType = typeof(VueWatchEntries<>).MakeGenericType(typeof(int));
        var handlerOptionsType = typeof(VueWatchHandlerOptions<>).MakeGenericType(typeof(int));
        var cleanupOptionsType = typeof(VueWatchCleanupHandlerOptions<>).MakeGenericType(typeof(int));
        var watchEntryImplicitInputs = watchEntryType
            .GetMethods(BindingFlags.Public | BindingFlags.Static)
            .Where(static method => method.Name == "op_Implicit" && method.ReturnType.IsGenericType && method.ReturnType.GetGenericTypeDefinition() == typeof(VueWatchEntry<>))
            .Select(static method => method.GetParameters().Single().ParameterType)
            .ToArray();
        var watchEntriesImplicitInputs = watchEntriesType
            .GetMethods(BindingFlags.Public | BindingFlags.Static)
            .Where(static method => method.Name == "op_Implicit" && method.ReturnType.IsGenericType && method.ReturnType.GetGenericTypeDefinition() == typeof(VueWatchEntries<>))
            .Select(static method => method.GetParameters().Single().ParameterType)
            .ToArray();

        Assert.IsNotNull(watch);
        Assert.IsNotNull(indexer);
        Assert.AreEqual(typeof(VueProps), watch!.PropertyType.UnwrapNullable());
        Assert.AreEqual(
            typeof(VueWatchDeclaration<int>),
            indexer!.PropertyType.UnwrapNullable());
        Assert.IsNotNull(typeof(VueWatchDeclaration<int>).GetCustomAttribute<System.Runtime.CompilerServices.UnionAttribute>());
        Assert.IsTrue(typeof(System.Runtime.CompilerServices.IUnion).IsAssignableFrom(typeof(VueWatchDeclaration<int>)));
        CollectionAssert.AreEqual(new[] { typeof(string) }, indexer.GetIndexParameters().Select(static parameter => parameter.ParameterType).ToArray());
        Assert.AreEqual(7, addMethods.Length);
        Assert.IsTrue(addMethods.Any(method =>
            method.GetParameters().Select(static parameter => parameter.ParameterType).SequenceEqual(new[] { typeof(string), typeof(string) })));
        Assert.IsTrue(addMethods.Any(method =>
            method.GetParameters().Select(static parameter => parameter.ParameterType).SequenceEqual(new[] { typeof(string), typeof(Action<int, int>) })));
        Assert.IsTrue(addMethods.Any(method =>
            method.GetParameters().Select(static parameter => parameter.ParameterType).SequenceEqual(new[] { typeof(string), typeof(VueWatchCleanupCallback<int>) })));
        Assert.IsTrue(addMethods.Any(method =>
            method.GetParameters().Select(static parameter => parameter.ParameterType).SequenceEqual(new[] { typeof(string), typeof(VueWatchHandlerOptions<int>) })));
        Assert.IsTrue(addMethods.Any(method =>
            method.GetParameters().Select(static parameter => parameter.ParameterType).SequenceEqual(new[] { typeof(string), typeof(VueWatchCleanupHandlerOptions<int>) })));
        Assert.IsTrue(addMethods.Any(method =>
            method.GetParameters().Select(static parameter => parameter.ParameterType).SequenceEqual(new[] { typeof(string), typeof(VueWatchNamedHandlerOptions) })));
        Assert.IsTrue(addMethods.Any(method =>
            method.GetParameters().Select(static parameter => parameter.ParameterType).SequenceEqual(new[] { typeof(string), watchEntriesType })));
        CollectionAssert.AreEquivalent(
            new[]
            {
                typeof(string),
                typeof(Action<int, int>),
                typeof(VueWatchCleanupCallback<int>),
                typeof(VueWatchHandlerOptions<int>),
                typeof(VueWatchCleanupHandlerOptions<int>),
                typeof(VueWatchNamedHandlerOptions)
            },
            watchEntryImplicitInputs);
        CollectionAssert.AreEquivalent(
            new[]
            {
                typeof(string[]),
                typeof(Action<int, int>[]),
                typeof(VueWatchCleanupCallback<int>[]),
                typeof(VueWatchHandlerOptions<int>[]),
                typeof(VueWatchCleanupHandlerOptions<int>[]),
                typeof(VueWatchNamedHandlerOptions[]),
                typeof(VueWatchEntry<int>[])
            },
            watchEntriesImplicitInputs);
        Assert.AreEqual(typeof(Action<int, int>), handlerOptionsType.GetProperty(nameof(VueWatchHandlerOptions<int>.Handler))!.PropertyType);
        Assert.AreEqual(typeof(VueWatchCleanupCallback<int>), cleanupOptionsType.GetProperty(nameof(VueWatchCleanupHandlerOptions<int>.Handler))!.PropertyType);
        Assert.AreEqual(typeof(string), typeof(VueWatchNamedHandlerOptions).GetProperty(nameof(VueWatchNamedHandlerOptions.Handler))!.PropertyType);
        CollectionAssert.Contains(
            watch.CustomAttributes.Select(static attribute => attribute.AttributeType.Name).ToArray(),
            "DescriptionAttribute");

        AssertNotObject(typeof(VueWatchRegistry<int>), "VueWatchRegistry<int>");
        AssertNotObject(watchEntryType, "VueWatchEntry<int>");
        AssertNotObject(watchEntriesType, "VueWatchEntries<int>");
        AssertNotObject(typeof(VueWatchHandlerOptions<int>), "VueWatchHandlerOptions<int>");
        AssertNotObject(typeof(VueWatchCleanupHandlerOptions<int>), "VueWatchCleanupHandlerOptions<int>");
        AssertNotObject(typeof(VueWatchNamedHandlerOptions), nameof(VueWatchNamedHandlerOptions));
        AssertNotObject(watch.PropertyType, "VueComponentDefinition.Watch");
    }

    [TestMethod]
    public void Vue_H_UsesTypedComponentSlotContracts()
    {
        var slotInvoke = typeof(VueSlotCallback).GetMethod("Invoke", BindingFlags.Public | BindingFlags.Instance);
        var scopedSlotInvoke = typeof(VueSlotCallback<>)
            .MakeGenericType(typeof(string))
            .GetMethod("Invoke", BindingFlags.Public | BindingFlags.Instance);
        var slotIndexer = typeof(VueSlots).GetProperty("Item", BindingFlags.Public | BindingFlags.Instance);
        var slotOverloads = typeof(Vue3)
            .GetMethods(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly)
            .Where(static method => method.Name == nameof(Vue3.H))
            .ToArray();

        Assert.IsNotNull(slotInvoke);
        Assert.IsNotNull(scopedSlotInvoke);
        Assert.AreEqual(typeof(IVNode), slotInvoke.ReturnType);
        Assert.AreEqual(typeof(IVNode), scopedSlotInvoke.ReturnType);
        CollectionAssert.AreEqual(
            new[] { typeof(string) },
            scopedSlotInvoke.GetParameters().Select(static parameter => parameter.ParameterType).ToArray());
        Assert.IsFalse(typeof(VueSlots).IsAbstract);
        Assert.IsNotNull(slotIndexer);
        Assert.AreEqual(typeof(VueSlotCallback), slotIndexer.PropertyType.UnwrapNullable());
        CollectionAssert.AreEqual(new[] { typeof(string) }, slotIndexer.GetIndexParameters().Select(static parameter => parameter.ParameterType).ToArray());
        Assert.IsTrue(slotOverloads.Any(static method =>
        {
            var parameters = method.GetParameters();
            return !method.IsGenericMethodDefinition &&
                   parameters.Length == 2 &&
                   parameters[0].ParameterType == typeof(ECMAScript.Vue3.IVueComponent) &&
                   parameters[1].ParameterType == typeof(VueSlots);
        }));
        Assert.IsTrue(slotOverloads.Any(static method =>
        {
            var parameters = method.GetParameters();
            return !method.IsGenericMethodDefinition &&
                   parameters.Length == 3 &&
                   parameters[0].ParameterType == typeof(ECMAScript.Vue3.IVueComponent) &&
                   parameters[1].ParameterType == typeof(VueProps) &&
                   parameters[2].ParameterType == typeof(VueSlots);
        }));
        Assert.IsTrue(slotOverloads.Any(static method =>
        {
            if (!method.IsGenericMethodDefinition || method.GetGenericArguments().Length != 1)
                return false;

            var parameters = method.GetParameters();
            return parameters.Length == 2 &&
                   parameters[0].ParameterType.IsGenericType &&
                   parameters[0].ParameterType.GetGenericTypeDefinition() == typeof(ECMAScript.Vue3.IVueSlotComponent<>) &&
                   parameters[1].ParameterType.IsGenericParameter;
        }));
        Assert.IsTrue(slotOverloads.Any(static method =>
        {
            if (!method.IsGenericMethodDefinition || method.GetGenericArguments().Length != 2)
                return false;

            var parameters = method.GetParameters();
            return parameters.Length == 2 &&
                   parameters[0].ParameterType.IsGenericType &&
                   parameters[0].ParameterType.GetGenericTypeDefinition() == typeof(ECMAScript.Vue3.IVueComponent<,>) &&
                   parameters[1].ParameterType.IsGenericParameter;
        }));
        Assert.IsTrue(slotOverloads.Any(static method =>
        {
            if (!method.IsGenericMethodDefinition || method.GetGenericArguments().Length != 2)
                return false;

            var parameters = method.GetParameters();
            return parameters.Length == 3 &&
                   parameters[0].ParameterType.IsGenericType &&
                   parameters[0].ParameterType.GetGenericTypeDefinition() == typeof(ECMAScript.Vue3.IVueComponent<,>) &&
                   parameters[1].ParameterType.IsGenericParameter &&
                   parameters[2].ParameterType.IsGenericParameter;
        }));
    }

    [TestMethod]
    public void Vue_H_ExposesChildOverloads()
    {
        var overloads = typeof(Vue3)
            .GetMethods(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly)
            .Where(static method => method.Name == nameof(Vue3.H) && !method.IsGenericMethodDefinition)
            .Select(static method => method.GetParameters().Select(static parameter => parameter.ParameterType).ToArray())
            .ToArray();

        static bool HasOverload(Type[][] overloads, params Type[] signature)
            => overloads.Any(parameters => parameters.SequenceEqual(signature));

        Assert.IsTrue(HasOverload(overloads, typeof(string), typeof(IVNode)));
        Assert.IsTrue(HasOverload(overloads, typeof(string), typeof(IVNode[])));
        Assert.IsTrue(HasOverload(overloads, typeof(string), typeof(VueChild)));
        Assert.IsTrue(HasOverload(overloads, typeof(string), typeof(VueProps), typeof(IVNode)));
        Assert.IsTrue(HasOverload(overloads, typeof(string), typeof(VueProps), typeof(IVNode[])));
        Assert.IsTrue(HasOverload(overloads, typeof(string), typeof(VueProps), typeof(VueChild)));
        Assert.IsTrue(HasOverload(overloads, typeof(ECMAScript.Vue3.IVueComponent), typeof(IVNode)));
        Assert.IsTrue(HasOverload(overloads, typeof(ECMAScript.Vue3.IVueComponent), typeof(IVNode[])));
        Assert.IsTrue(HasOverload(overloads, typeof(ECMAScript.Vue3.IVueComponent), typeof(VueChild)));
        Assert.IsTrue(HasOverload(overloads, typeof(ECMAScript.Vue3.IVueComponent), typeof(VueProps), typeof(IVNode)));
        Assert.IsTrue(HasOverload(overloads, typeof(ECMAScript.Vue3.IVueComponent), typeof(VueProps), typeof(IVNode[])));
        Assert.IsTrue(HasOverload(overloads, typeof(ECMAScript.Vue3.IVueComponent), typeof(VueProps), typeof(VueChild)));

        Assert.IsFalse(overloads.Any(static parameters =>
            parameters.Any(static parameter =>
                parameter.IsGenericType &&
                parameter.GetGenericTypeDefinition().Name.StartsWith("Either`", StringComparison.Ordinal))));
    }

    [TestMethod]
    public void Vue_H_ExposesTypedDefaultSlotChildOverloads()
    {
        var overloads = typeof(Vue3)
            .GetMethods(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly)
            .Where(static method => method.Name == nameof(Vue3.H) && method.IsGenericMethodDefinition)
            .ToArray();

        static bool HasGenericOverload(MethodInfo[] methods, int genericArity, params Func<ParameterInfo[], bool>[] predicates)
            => methods.Any(method =>
            {
                if (method.GetGenericArguments().Length != genericArity)
                    return false;

                var parameters = method.GetParameters();
                return predicates.Any(predicate => predicate(parameters));
            });

        Assert.IsTrue(HasGenericOverload(
            overloads,
            1,
            parameters => parameters.Length == 2 &&
                          parameters[0].ParameterType.IsGenericType &&
                          parameters[0].ParameterType.GetGenericTypeDefinition() == typeof(ECMAScript.Vue3.IVueComponent<>) &&
                          parameters[1].ParameterType == typeof(IVNode)));

        Assert.IsTrue(HasGenericOverload(
            overloads,
            1,
            parameters => parameters.Length == 2 &&
                          parameters[0].ParameterType.IsGenericType &&
                          parameters[0].ParameterType.GetGenericTypeDefinition() == typeof(ECMAScript.Vue3.IVueComponent<>) &&
                          parameters[1].ParameterType == typeof(IVNode[])));

        Assert.IsTrue(HasGenericOverload(
            overloads,
            1,
            parameters => parameters.Length == 2 &&
                          parameters[0].ParameterType.IsGenericType &&
                          parameters[0].ParameterType.GetGenericTypeDefinition() == typeof(ECMAScript.Vue3.IVueComponent<>) &&
                          parameters[1].ParameterType == typeof(VueChild)));

        Assert.IsTrue(HasGenericOverload(
            overloads,
            1,
            parameters => parameters.Length == 3 &&
                          parameters[0].ParameterType.IsGenericType &&
                          parameters[0].ParameterType.GetGenericTypeDefinition() == typeof(ECMAScript.Vue3.IVueComponent<>) &&
                          parameters[1].ParameterType.IsGenericParameter &&
                          parameters[2].ParameterType == typeof(IVNode)));

        Assert.IsTrue(HasGenericOverload(
            overloads,
            1,
            parameters => parameters.Length == 3 &&
                          parameters[0].ParameterType.IsGenericType &&
                          parameters[0].ParameterType.GetGenericTypeDefinition() == typeof(ECMAScript.Vue3.IVueComponent<>) &&
                          parameters[1].ParameterType.IsGenericParameter &&
                          parameters[2].ParameterType == typeof(IVNode[])));

        Assert.IsTrue(HasGenericOverload(
            overloads,
            1,
            parameters => parameters.Length == 3 &&
                          parameters[0].ParameterType.IsGenericType &&
                          parameters[0].ParameterType.GetGenericTypeDefinition() == typeof(ECMAScript.Vue3.IVueComponent<>) &&
                          parameters[1].ParameterType.IsGenericParameter &&
                          parameters[2].ParameterType == typeof(VueChild)));

        Assert.IsTrue(HasGenericOverload(
            overloads,
            1,
            parameters => parameters.Length == 3 &&
                          parameters[0].ParameterType.IsGenericType &&
                          parameters[0].ParameterType.GetGenericTypeDefinition() == typeof(ECMAScript.Vue3.IVueComponent<>) &&
                          parameters[1].ParameterType.IsGenericType &&
                          parameters[1].ParameterType.GetGenericTypeDefinition() == typeof(VueObject<>) &&
                          parameters[2].ParameterType == typeof(IVNode)));

        Assert.IsTrue(HasGenericOverload(
            overloads,
            1,
            parameters => parameters.Length == 3 &&
                          parameters[0].ParameterType.IsGenericType &&
                          parameters[0].ParameterType.GetGenericTypeDefinition() == typeof(ECMAScript.Vue3.IVueComponent<>) &&
                          parameters[1].ParameterType.IsGenericType &&
                          parameters[1].ParameterType.GetGenericTypeDefinition() == typeof(VueObject<>) &&
                          parameters[2].ParameterType == typeof(IVNode[])));

        Assert.IsTrue(HasGenericOverload(
            overloads,
            1,
            parameters => parameters.Length == 3 &&
                          parameters[0].ParameterType.IsGenericType &&
                          parameters[0].ParameterType.GetGenericTypeDefinition() == typeof(ECMAScript.Vue3.IVueComponent<>) &&
                          parameters[1].ParameterType.IsGenericType &&
                          parameters[1].ParameterType.GetGenericTypeDefinition() == typeof(VueObject<>) &&
                          parameters[2].ParameterType == typeof(VueChild)));

        Assert.IsTrue(HasGenericOverload(
            overloads,
            1,
            parameters => parameters.Length == 2 &&
                          parameters[0].ParameterType.IsGenericType &&
                          parameters[0].ParameterType.GetGenericTypeDefinition() == typeof(ECMAScript.Vue3.IVueSlotComponent<>) &&
                          parameters[1].ParameterType == typeof(IVNode)));

        Assert.IsTrue(HasGenericOverload(
            overloads,
            1,
            parameters => parameters.Length == 2 &&
                          parameters[0].ParameterType.IsGenericType &&
                          parameters[0].ParameterType.GetGenericTypeDefinition() == typeof(ECMAScript.Vue3.IVueSlotComponent<>) &&
                          parameters[1].ParameterType == typeof(IVNode[])));

        Assert.IsTrue(HasGenericOverload(
            overloads,
            1,
            parameters => parameters.Length == 2 &&
                          parameters[0].ParameterType.IsGenericType &&
                          parameters[0].ParameterType.GetGenericTypeDefinition() == typeof(ECMAScript.Vue3.IVueSlotComponent<>) &&
                          parameters[1].ParameterType == typeof(VueChild)));

        Assert.IsTrue(HasGenericOverload(
            overloads,
            2,
            parameters => parameters.Length == 2 &&
                          parameters[0].ParameterType.IsGenericType &&
                          parameters[0].ParameterType.GetGenericTypeDefinition() == typeof(ECMAScript.Vue3.IVueComponent<,>) &&
                          parameters[1].ParameterType == typeof(IVNode)));

        Assert.IsTrue(HasGenericOverload(
            overloads,
            2,
            parameters => parameters.Length == 2 &&
                          parameters[0].ParameterType.IsGenericType &&
                          parameters[0].ParameterType.GetGenericTypeDefinition() == typeof(ECMAScript.Vue3.IVueComponent<,>) &&
                          parameters[1].ParameterType == typeof(IVNode[])));

        Assert.IsTrue(HasGenericOverload(
            overloads,
            2,
            parameters => parameters.Length == 2 &&
                          parameters[0].ParameterType.IsGenericType &&
                          parameters[0].ParameterType.GetGenericTypeDefinition() == typeof(ECMAScript.Vue3.IVueComponent<,>) &&
                          parameters[1].ParameterType == typeof(VueChild)));

        Assert.IsTrue(HasGenericOverload(
            overloads,
            2,
            parameters => parameters.Length == 3 &&
                          parameters[0].ParameterType.IsGenericType &&
                          parameters[0].ParameterType.GetGenericTypeDefinition() == typeof(ECMAScript.Vue3.IVueComponent<,>) &&
                          parameters[1].ParameterType.IsGenericParameter &&
                          parameters[2].ParameterType == typeof(IVNode)));

        Assert.IsTrue(HasGenericOverload(
            overloads,
            2,
            parameters => parameters.Length == 3 &&
                          parameters[0].ParameterType.IsGenericType &&
                          parameters[0].ParameterType.GetGenericTypeDefinition() == typeof(ECMAScript.Vue3.IVueComponent<,>) &&
                          parameters[1].ParameterType.IsGenericParameter &&
                          parameters[2].ParameterType == typeof(IVNode[])));

        Assert.IsTrue(HasGenericOverload(
            overloads,
            2,
            parameters => parameters.Length == 3 &&
                          parameters[0].ParameterType.IsGenericType &&
                          parameters[0].ParameterType.GetGenericTypeDefinition() == typeof(ECMAScript.Vue3.IVueComponent<,>) &&
                          parameters[1].ParameterType.IsGenericParameter &&
                          parameters[2].ParameterType == typeof(VueChild)));

        Assert.IsTrue(HasGenericOverload(
            overloads,
            2,
            parameters => parameters.Length == 3 &&
                          parameters[0].ParameterType.IsGenericType &&
                          parameters[0].ParameterType.GetGenericTypeDefinition() == typeof(ECMAScript.Vue3.IVueComponent<,>) &&
                          parameters[1].ParameterType.IsGenericType &&
                          parameters[1].ParameterType.GetGenericTypeDefinition() == typeof(VueObject<>) &&
                          parameters[2].ParameterType == typeof(IVNode)));

        Assert.IsTrue(HasGenericOverload(
            overloads,
            2,
            parameters => parameters.Length == 3 &&
                          parameters[0].ParameterType.IsGenericType &&
                          parameters[0].ParameterType.GetGenericTypeDefinition() == typeof(ECMAScript.Vue3.IVueComponent<,>) &&
                          parameters[1].ParameterType.IsGenericType &&
                          parameters[1].ParameterType.GetGenericTypeDefinition() == typeof(VueObject<>) &&
                          parameters[2].ParameterType == typeof(IVNode[])));

        Assert.IsTrue(HasGenericOverload(
            overloads,
            2,
            parameters => parameters.Length == 3 &&
                          parameters[0].ParameterType.IsGenericType &&
                          parameters[0].ParameterType.GetGenericTypeDefinition() == typeof(ECMAScript.Vue3.IVueComponent<,>) &&
                          parameters[1].ParameterType.IsGenericType &&
                          parameters[1].ParameterType.GetGenericTypeDefinition() == typeof(VueObject<>) &&
                          parameters[2].ParameterType == typeof(VueChild)));

        Assert.IsFalse(overloads.Any(static method =>
            method.GetParameters().Any(static parameter =>
                parameter.ParameterType.IsGenericType &&
                parameter.ParameterType.GetGenericTypeDefinition().Name.StartsWith("Either`", StringComparison.Ordinal))));
    }

    [TestMethod]
    public void Vue_H_ExposesTypedVueObjectPropsOverloads()
    {
        var overloads = typeof(Vue3)
            .GetMethods(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly)
            .Where(static method => method.Name == nameof(Vue3.H) && method.IsGenericMethodDefinition)
            .ToArray();

        static bool HasGenericOverload(MethodInfo[] methods, int genericArity, Func<ParameterInfo[], bool> predicate)
            => methods.Any(method =>
                method.GetGenericArguments().Length == genericArity &&
                predicate(method.GetParameters()));

        Assert.IsTrue(HasGenericOverload(
            overloads,
            1,
            parameters => parameters.Length == 2 &&
                          parameters[0].ParameterType.IsGenericType &&
                          parameters[0].ParameterType.GetGenericTypeDefinition() == typeof(ECMAScript.Vue3.IVueComponent<>) &&
                          parameters[1].ParameterType.IsGenericType &&
                          parameters[1].ParameterType.GetGenericTypeDefinition() == typeof(VueObject<>)));

        Assert.IsTrue(HasGenericOverload(
            overloads,
            2,
            parameters => parameters.Length == 2 &&
                          parameters[0].ParameterType.IsGenericType &&
                          parameters[0].ParameterType.GetGenericTypeDefinition() == typeof(ECMAScript.Vue3.IVueComponent<,>) &&
                          parameters[1].ParameterType.IsGenericType &&
                          parameters[1].ParameterType.GetGenericTypeDefinition() == typeof(VueObject<>)));

        Assert.IsTrue(HasGenericOverload(
            overloads,
            2,
            parameters => parameters.Length == 3 &&
                          parameters[0].ParameterType.IsGenericType &&
                          parameters[0].ParameterType.GetGenericTypeDefinition() == typeof(ECMAScript.Vue3.IVueComponent<,>) &&
                          parameters[1].ParameterType.IsGenericType &&
                          parameters[1].ParameterType.GetGenericTypeDefinition() == typeof(VueObject<>) &&
                          parameters[2].ParameterType == typeof(VueChild)));

        Assert.IsTrue(HasGenericOverload(
            overloads,
            2,
            parameters => parameters.Length == 3 &&
                          parameters[0].ParameterType.IsGenericType &&
                          parameters[0].ParameterType.GetGenericTypeDefinition() == typeof(ECMAScript.Vue3.IVueComponent<,>) &&
                          parameters[1].ParameterType.IsGenericType &&
                          parameters[1].ParameterType.GetGenericTypeDefinition() == typeof(VueObject<>) &&
                          parameters[2].ParameterType.IsGenericParameter));
    }

    [TestMethod]
    public void Vue_BindThis_ExposesThisBoundCallbackBridge()
    {
        var methods = typeof(Vue3)
            .GetMethods(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly)
            .Where(static method => method.Name == nameof(Vue3.BindThis))
            .ToArray();
        const string bindThisInlineTemplate = "((__cb) => function(){ return __cb(this, ...arguments); })(__arg1)";

        static bool TypeMatches(Type actual, Type expected)
        {
            if (expected.IsGenericTypeDefinition)
                return actual.IsGenericType && actual.GetGenericTypeDefinition() == expected;

            return actual == expected;
        }

        static bool HasSignature(MethodInfo[] methods, Type returnType, int genericArity, params Type[] parameterTypes)
            => methods.Any(method =>
                TypeMatches(method.ReturnType, returnType) &&
                method.GetGenericArguments().Length == genericArity &&
                method.GetParameters().Length == parameterTypes.Length &&
                method.GetParameters()
                    .Select(static parameter => parameter.ParameterType)
                    .Zip(parameterTypes, TypeMatches)
                    .All(static matched => matched));

        Assert.IsTrue(HasSignature(methods, typeof(VueDataCallback), 1, typeof(VueThisDataCallback<>)));
        Assert.IsTrue(HasSignature(methods, typeof(Action), 1, typeof(VueThisAction<>)));
        Assert.IsTrue(HasSignature(methods, typeof(Action<>), 2, typeof(VueThisAction<,>)));
        Assert.IsTrue(HasSignature(methods, typeof(Action<,>), 3, typeof(VueThisAction<,,>)));
        Assert.IsTrue(HasSignature(methods, typeof(Action<,,>), 4, typeof(VueThisAction<,,,>)));
        Assert.IsTrue(HasSignature(methods, typeof(VueWatchCleanupCallback<>), 2, typeof(VueThisWatchCleanupCallback<,>)));
        Assert.IsTrue(HasSignature(methods, typeof(Func<>), 2, typeof(VueThisFunc<,>)));
        Assert.IsTrue(HasSignature(methods, typeof(Func<,>), 3, typeof(VueThisFunc<,,>)));
        Assert.IsTrue(HasSignature(methods, typeof(Func<,,>), 4, typeof(VueThisFunc<,,,>)));
        Assert.IsTrue(HasSignature(methods, typeof(Func<,,,>), 5, typeof(VueThisFunc<,,,,>)));
        Assert.AreEqual(10, methods.Length);

        foreach (var method in methods)
        {
            var inline = method.GetCustomAttribute<ECMAScriptInlineAttribute>();
            Assert.IsNotNull(inline, $"Vue3.{nameof(Vue3.BindThis)} overload must declare [{nameof(ECMAScriptInlineAttribute)}].");
            Assert.AreEqual(bindThisInlineTemplate, inline.RawFuncCode, $"Vue3.{nameof(Vue3.BindThis)} overload inline template drifted.");
        }

        AssertNotObject(typeof(VueThisDataCallback<>), "VueThisDataCallback<TThis>");
        AssertNotObject(typeof(VueThisAction<>), "VueThisAction<TThis>");
        AssertNotObject(typeof(VueThisAction<,>), "VueThisAction<TThis,T1>");
        AssertNotObject(typeof(VueThisAction<,,>), "VueThisAction<TThis,T1,T2>");
        AssertNotObject(typeof(VueThisAction<,,,>), "VueThisAction<TThis,T1,T2,T3>");
        AssertNotObject(typeof(VueThisWatchCleanupCallback<,>), "VueThisWatchCleanupCallback<TThis,TValue>");
        AssertNotObject(typeof(VueThisFunc<,>), "VueThisFunc<TThis,TResult>");
        AssertNotObject(typeof(VueThisFunc<,,>), "VueThisFunc<TThis,T1,TResult>");
        AssertNotObject(typeof(VueThisFunc<,,,>), "VueThisFunc<TThis,T1,T2,TResult>");
        AssertNotObject(typeof(VueThisFunc<,,,,>), "VueThisFunc<TThis,T1,T2,T3,TResult>");
    }

    [TestMethod]
    public void Vue_H_UsesCanonicalVueChildBridge()
    {
        var childType = typeof(VueChild);
        var implicitOperators = childType
            .GetMethods(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly)
            .Where(static method => method.Name == "op_Implicit" &&
                                    method.ReturnType == typeof(VueChild) &&
                                    method.GetParameters().Length == 1)
            .Select(static method => method.GetParameters()[0].ParameterType)
            .ToArray();

        Assert.IsFalse(childType.IsAbstract);
        CollectionAssert.Contains(implicitOperators, typeof(string));
        CollectionAssert.Contains(implicitOperators, typeof(Number));
        CollectionAssert.Contains(implicitOperators, typeof(bool));
        CollectionAssert.Contains(implicitOperators, typeof(IVNode[]));
        CollectionAssert.Contains(implicitOperators, typeof(byte));
        CollectionAssert.Contains(implicitOperators, typeof(sbyte));
        CollectionAssert.Contains(implicitOperators, typeof(short));
        CollectionAssert.Contains(implicitOperators, typeof(ushort));
        CollectionAssert.Contains(implicitOperators, typeof(int));
        CollectionAssert.Contains(implicitOperators, typeof(uint));
        CollectionAssert.Contains(implicitOperators, typeof(long));
        CollectionAssert.Contains(implicitOperators, typeof(ulong));
        CollectionAssert.Contains(implicitOperators, typeof(float));
        CollectionAssert.Contains(implicitOperators, typeof(double));
        CollectionAssert.Contains(implicitOperators, typeof(decimal));
        CollectionAssert.DoesNotContain(implicitOperators, typeof(object));
        CollectionAssert.DoesNotContain(implicitOperators, typeof(IVNode));

        AssertNotObject(typeof(VueChild), nameof(VueChild));
    }

    [TestMethod]
    public void Vue_DefaultSlotSugar_DoesNotDependOnJazorAttributes()
    {
        var vueType = typeof(Vue3);
        var hOverloads = vueType
            .GetMethods(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly)
            .Where(static method => method.Name == nameof(Vue3.H))
            .ToArray();

        Assert.IsFalse(HasAttribute(vueType, "JazorAttribute"));

        foreach (var method in hOverloads.Where(IsVueDefaultSlotSugarOverload))
            Assert.IsFalse(HasAttribute(method, "JazorAttribute"), method.ToString());

        static bool IsVueDefaultSlotSugarOverload(MethodInfo method)
        {
            var parameters = method.GetParameters();
            if (parameters.Length is not (2 or 3))
                return false;

            var receiverType = parameters[0].ParameterType;
            var childType = parameters[^1].ParameterType;
            if (receiverType != typeof(ECMAScript.Vue3.IVueComponent) &&
                !(receiverType.IsGenericType &&
                  (receiverType.GetGenericTypeDefinition() == typeof(ECMAScript.Vue3.IVueSlotComponent<>) ||
                   receiverType.GetGenericTypeDefinition() == typeof(ECMAScript.Vue3.IVueComponent<,>))))
            {
                return false;
            }

            return childType == typeof(IVNode) || childType == typeof(VueChild);
        }
    }

    [TestMethod]
    public void Vuetify_ComponentExports_AreConcreteComponentTypes()
    {
        AssertComponentExportsMatchRegistry(typeof(VuetifyComponents), typeof(VuetifyComponentRegistry));
        AssertComponentExportsMatchRegistry(typeof(VuetifyLabsComponents), typeof(VuetifyLabsComponentRegistry));
    }

    private static void AssertComponentExportsMatchRegistry(Type exportHost, Type registryHost)
    {
        var exportedComponents = exportHost
            .GetProperties(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly)
            .OrderBy(static property => property.Name, StringComparer.Ordinal)
            .ToArray();
        var registryProperties = registryHost
            .GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
            .ToDictionary(static property => property.Name, StringComparer.Ordinal);

        Assert.IsTrue(exportedComponents.Length > 0);
        foreach (var property in exportedComponents)
        {
            Assert.IsTrue(typeof(ECMAScript.Vue3.IVueComponent).IsAssignableFrom(property.PropertyType), property.Name);
            Assert.IsTrue(typeof(IVuetifyComponent).IsAssignableFrom(property.PropertyType), property.Name);
            Assert.AreEqual(property.PropertyType, registryProperties[property.Name].PropertyType.UnwrapNullable(), property.Name);
        }
    }

    [TestMethod]
    public void Vuetify_DirectiveExports_AreConcreteDirectiveTypes()
    {
        var exportedDirectives = typeof(VuetifyDirectives)
            .GetProperties(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly)
            .OrderBy(static property => property.Name, StringComparer.Ordinal)
            .ToArray();
        var registryProperties = typeof(VuetifyDirectiveRegistry)
            .GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
            .ToDictionary(static property => property.Name, StringComparer.Ordinal);

        Assert.IsTrue(exportedDirectives.Length > 0);
        foreach (var property in exportedDirectives)
        {
            Assert.IsTrue(typeof(VuetifyDirective).IsAssignableFrom(property.PropertyType), property.Name);
            Assert.IsTrue(typeof(VueDirective).IsAssignableFrom(property.PropertyType), property.Name);
            Assert.AreEqual(property.PropertyType, registryProperties[property.Name].PropertyType.UnwrapNullable(), property.Name);
        }
    }

    [TestMethod]
    public void Vuetify_OptionsAndRegistries_DoNotExposeObjectProperties()
    {
        var optionTypes = typeof(VuetifyOptions).Assembly
            .GetTypes()
            .Where(static type =>
                type.Namespace == "ECMAScript.Vuetify" &&
                (type.Name.EndsWith("Options", StringComparison.Ordinal) ||
                 type.Name.EndsWith("Registry", StringComparison.Ordinal)))
            .OrderBy(static type => type.FullName, StringComparer.Ordinal)
            .ToArray();

        Assert.IsTrue(optionTypes.Length > 0);
        foreach (var property in optionTypes.SelectMany(static type => type.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)))
        {
            if (IsUnionValueProperty(property))
                continue;

            Assert.AreNotEqual(typeof(object), property.PropertyType.UnwrapNullable(), $"{property.DeclaringType?.Name}.{property.Name}");
        }
    }

    [TestMethod]
    public void Vuetify_AuthoringComponents_ExposeOnlyAdditionalAttributesAsObjectSink()
    {
        var componentTypes = typeof(Vuetify).Assembly
            .GetTypes()
            .Where(static type =>
                type.Namespace == "ECMAScript.Vuetify" &&
                type.IsClass &&
                !type.IsAbstract &&
                typeof(Microsoft.AspNetCore.Components.ComponentBase).IsAssignableFrom(type) &&
                typeof(IVueLibraryComponent).IsAssignableFrom(type))
            .OrderBy(static type => type.FullName, StringComparer.Ordinal)
            .ToArray();

        Assert.IsTrue(componentTypes.Length > 0);
        CollectionAssert.AreEquivalent(
            GetVuetifyRuntimeComponentNames()
                .OrderBy(static name => name, StringComparer.Ordinal)
                .ToArray(),
            componentTypes
                .Select(static type => type.Name)
                .OrderBy(static name => name, StringComparer.Ordinal)
                .ToArray());

        foreach (var componentType in componentTypes)
        {
            var additionalAttributes = componentType.GetProperty("AdditionalAttributes", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);
            Assert.IsNotNull(additionalAttributes, componentType.FullName);
            Assert.AreEqual(
                typeof(IReadOnlyDictionary<string, object?>),
                additionalAttributes!.PropertyType.UnwrapNullable(),
                componentType.FullName);

            var parameter = additionalAttributes.GetCustomAttribute<Microsoft.AspNetCore.Components.ParameterAttribute>();
            Assert.IsNotNull(parameter, componentType.FullName);
            Assert.IsTrue(parameter!.CaptureUnmatchedValues, componentType.FullName);

            foreach (var property in componentType.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly))
            {
                if (property.Name == "AdditionalAttributes")
                    continue;

                AssertNotObject(property.PropertyType, $"{componentType.Name}.{property.Name}");
            }
        }
    }

    [TestMethod]
    public void Vuetify_ComponentNames_UseMemberLevelGeneralMetadata()
    {
        var componentTypes = typeof(Vuetify).Assembly
            .GetTypes()
            .Where(static type =>
                type.Namespace == "ECMAScript.Vuetify" &&
                typeof(ComponentBase).IsAssignableFrom(type))
            .ToArray();

        Assert.IsFalse(componentTypes.Any(static type =>
            type.GetCustomAttributes<ECMAScript.VueContract.VuePropAttribute>(inherit: false).Any()));
        Assert.IsFalse(componentTypes.Any(static type =>
            type.GetCustomAttributes<ECMAScript.VueContract.VueSlotAttribute>(inherit: false).Any()));

        var names = new (Type Type, string Property, string RuntimeName)[]
        {
            (typeof(VAlert), nameof(VAlert.CssClass), "class"),
            (typeof(VAlert), nameof(VAlert.CssStyle), "style"),
            (typeof(VAutocomplete), nameof(VAutocomplete.SelectedValue), "modelValue"),
            (typeof(VDatePicker), nameof(VDatePicker.HeaderText), "header"),
            (typeof(VImg), nameof(VImg.CrossOrigin), "crossorigin"),
            (typeof(VImg), nameof(VImg.ReferrerPolicy), "referrerpolicy"),
            (typeof(VDataTable), nameof(VDataTable.HeaderSelect), "header.data-table-select"),
            (typeof(VDataTable), nameof(VDataTable.HeaderExpand), "header.data-table-expand"),
            (typeof(VDataTable), nameof(VDataTable.BodyPrepend), "body.prepend"),
            (typeof(VDataTable), nameof(VDataTable.BodyAppend), "body.append"),
            (typeof(VDataTable), nameof(VDataTable.FooterPrepend), "footer.prepend")
        };

        foreach (var (type, propertyName, runtimeName) in names)
        {
            var property = type.GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance);
            Assert.IsNotNull(property, $"{type.Name}.{propertyName}");
            Assert.AreEqual(
                runtimeName,
                property!.GetCustomAttribute<ECMAScriptNameAttribute>()?.Name,
                $"{type.Name}.{propertyName}");
        }
    }

    [TestMethod]
    public void TDesign_AuthoringComponents_ExposeOnlyAdditionalAttributesAsObjectSink()
    {
        var componentTypes = typeof(TDesign).Assembly
            .GetTypes()
            .Where(static type =>
                type.Namespace == "ECMAScript.TDesign" &&
                type.IsClass &&
                !type.IsAbstract &&
                typeof(ComponentBase).IsAssignableFrom(type) &&
                typeof(IVueLibraryComponent).IsAssignableFrom(type))
            .OrderBy(static type => type.FullName, StringComparer.Ordinal)
            .ToArray();

        Assert.IsTrue(componentTypes.Length > 0);
        CollectionAssert.AreEquivalent(
            GetTDesignRuntimeComponentNames()
                .OrderBy(static name => name, StringComparer.Ordinal)
                .ToArray(),
            componentTypes
                .Select(static type => type.Name)
                .OrderBy(static name => name, StringComparer.Ordinal)
                .ToArray());

        foreach (var componentType in componentTypes)
        {
            var additionalAttributes = componentType.GetProperty("AdditionalAttributes", BindingFlags.Public | BindingFlags.Instance | BindingFlags.FlattenHierarchy);
            Assert.IsNotNull(additionalAttributes, componentType.FullName);
            Assert.AreEqual(
                typeof(IReadOnlyDictionary<string, object?>),
                additionalAttributes!.PropertyType.UnwrapNullable(),
                componentType.FullName);

            var parameter = additionalAttributes.GetCustomAttribute<ParameterAttribute>(inherit: true);
            Assert.IsNotNull(parameter, componentType.FullName);
            Assert.IsTrue(parameter!.CaptureUnmatchedValues, componentType.FullName);

            foreach (var property in componentType.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly))
            {
                if (property.Name == "AdditionalAttributes")
                    continue;

                AssertNotObject(property.PropertyType, $"{componentType.Name}.{property.Name}");
            }
        }
    }

    [TestMethod]
    public void TDesign_ComponentNames_UseMemberLevelGeneralMetadata()
    {
        var componentTypes = typeof(TDesign).Assembly
            .GetTypes()
            .Where(static type =>
                type.Namespace == "ECMAScript.TDesign" &&
                typeof(ComponentBase).IsAssignableFrom(type))
            .ToArray();

        Assert.IsFalse(componentTypes.Any(static type =>
            type.GetCustomAttributes<ECMAScript.VueContract.VuePropAttribute>(inherit: false).Any()));
        Assert.IsFalse(componentTypes.Any(static type =>
            type.GetCustomAttributes<ECMAScript.VueContract.VueSlotAttribute>(inherit: false).Any()));

        var names = new (Type Type, string Property, string RuntimeName)[]
        {
            (typeof(TButton), nameof(TButton.CssClass), "class"),
            (typeof(TButton), nameof(TButton.CssStyle), "style"),
            (typeof(TButton), nameof(TButton.Text), "content"),
            (typeof(TCard), nameof(TCard.BodyCssClass), "bodyClassName"),
            (typeof(TCard), nameof(TCard.HeaderCssStyle), "headerStyle"),
            (typeof(TAvatarGroup), nameof(TAvatarGroup.CollapseAvatar), "collapseAvatar"),
            (typeof(TLink), nameof(TLink.PrefixIcon), "prefixIcon"),
            (typeof(TLink), nameof(TLink.SuffixIcon), "suffixIcon")
        };

        foreach (var (type, propertyName, runtimeName) in names)
        {
            var property = type.GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance);
            Assert.IsNotNull(property, $"{type.Name}.{propertyName}");
            Assert.AreEqual(
                runtimeName,
                property!.GetCustomAttribute<ECMAScriptNameAttribute>(inherit: true)?.Name,
                $"{type.Name}.{propertyName}");
        }
    }

    [TestMethod]
    public void ElementPlus_GeneratedComponentNames_UseMemberLevelGeneralMetadata()
    {
        var componentTypes = typeof(ElementPlus).Assembly
            .GetTypes()
            .Where(static type =>
                type.Namespace == "ECMAScript.ElementPlus" &&
                typeof(ComponentBase).IsAssignableFrom(type))
            .ToArray();

        Assert.IsFalse(componentTypes.Any(static type =>
            type.GetCustomAttributes<ECMAScript.VueContract.VuePropAttribute>(inherit: false).Any()));
        Assert.IsFalse(componentTypes.Any(static type =>
            type.GetCustomAttributes<ECMAScript.VueContract.VueSlotAttribute>(inherit: false).Any()));
        Assert.IsFalse(componentTypes.Any(static type =>
            type.GetCustomAttributes<ECMAScript.VueContract.VueLibraryStyleAttribute>(inherit: false).Any()));
        Assert.IsFalse(componentTypes.Any(static type =>
            type.GetCustomAttributes<ECMAScript.VueContract.VueLibraryPluginRequirementAttribute>(inherit: false).Any()));

        Assert.AreEqual(
            "class",
            typeof(ElAffix).GetProperty(nameof(ElAffix.CssClass))!
                .GetCustomAttribute<ECMAScriptNameAttribute>(inherit: true)?.Name);
        Assert.AreEqual(
            "style",
            typeof(ElAffix).GetProperty(nameof(ElAffix.CssStyle))!
                .GetCustomAttribute<ECMAScriptNameAttribute>(inherit: true)?.Name);
        Assert.AreEqual(
            "title",
            typeof(ElAlert).GetProperty(nameof(ElAlert.TitleSlot))!
                .GetCustomAttribute<ECMAScriptNameAttribute>()?.Name);
        Assert.IsNull(
            typeof(ElAffix).GetProperty(nameof(ElAffix.Offset))!
                .GetCustomAttribute<ECMAScriptNameAttribute>());
    }

    private static IEnumerable<string> GetVuetifyRuntimeComponentNames()
        => GetVuetifyRuntimeComponentNames(typeof(VuetifyComponents))
            .Concat(GetVuetifyRuntimeComponentNames(typeof(VuetifyLabsComponents)));

    private static IEnumerable<string> GetVuetifyRuntimeComponentNames(Type exportHost)
        => exportHost
            .GetProperties(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly)
            .Where(static property => property.PropertyType == typeof(IVuetifyComponent))
            .Select(static property => property.Name);

    private static IEnumerable<string> GetTDesignRuntimeComponentNames()
        => typeof(TDesignComponents)
            .GetProperties(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly)
            .Where(static property => property.PropertyType == typeof(ITDesignComponent))
            .Select(static property => property.Name);

    [TestMethod]
    public void Vuetify_TaggedUnions_ExposePublicCreationMembers()
        => AssertTaggedUnionsExposePublicCreationMembers(
            typeof(Vuetify).Assembly,
            typeof(Vuetify).Namespace!);

    [TestMethod]
    public void VuetifyGridSpanValue_UsesNativeUnionWithoutLosingNumericAssignments()
    {
        AssertNet11UnionContract(typeof(VuetifyGridSpanValue), typeof(bool), typeof(Number), typeof(string));

        Assert.IsNotNull(typeof(VuetifyGridSpanValue).GetMethod(
            "op_Implicit",
            BindingFlags.Public | BindingFlags.Static,
            [typeof(decimal)]));
    }

    [TestMethod]
    public void VuetifyCalendarValueUnions_UseNativeUnionContracts()
    {
        var unionTypes = new[]
        {
            typeof(VuetifyCalendarDateValue),
            typeof(VuetifyCalendarAllowedDatesValue),
            typeof(VuetifyCalendarIntervalFormatValue)
        };

        AssertNet11UnionContract(typeof(VuetifyCalendarDateValue), typeof(Date), typeof(string), typeof(Number));
        AssertNet11UnionContract(
            typeof(VuetifyCalendarAllowedDatesValue),
            typeof(VuetifyCalendarDateValues),
            typeof(VuetifyCalendarAllowedDateResolver));
        AssertNet11UnionContract(
            typeof(VuetifyCalendarIntervalFormatValue),
            typeof(string),
            typeof(VuetifyCalendarIntervalFormatter));

        foreach (var unionType in unionTypes)
        {
            Assert.IsNull(unionType.GetMethod("From", BindingFlags.Public | BindingFlags.Static), unionType.FullName);
        }

        Assert.IsNotNull(typeof(VuetifyCalendarDateValue).GetMethod(
            "op_Implicit",
            BindingFlags.Public | BindingFlags.Static,
            [typeof(decimal)]));
        Assert.IsNotNull(typeof(VuetifyCalendarAllowedDatesValue).GetMethod(
            "op_Implicit",
            BindingFlags.Public | BindingFlags.Static,
            [typeof(Date[])]));
    }

    [TestMethod]
    public void VuetifyColorPickerValues_UseNativeUnionContracts()
    {
        var unionTypes = new[]
        {
            typeof(VuetifyColorPickerModes),
            typeof(VuetifyColorValue),
            typeof(VuetifyColorPickerSwatch),
            typeof(VuetifyColorPickerSwatches)
        };

        AssertNet11UnionContract(typeof(VuetifyColorPickerModes), typeof(VuetifyColorPickerMode[]));
        AssertNet11UnionContract(
            typeof(VuetifyColorValue),
            typeof(string),
            typeof(Number),
            typeof(VuetifyRgbColor),
            typeof(VuetifyHsvColor),
            typeof(VuetifyHslColor));
        AssertNet11UnionContract(typeof(VuetifyColorPickerSwatch), typeof(VuetifyColorValue[]));
        AssertNet11UnionContract(typeof(VuetifyColorPickerSwatches), typeof(VuetifyColorPickerSwatch[]));

        foreach (var unionType in unionTypes)
        {
            Assert.IsNull(unionType.GetMethod("From", BindingFlags.Public | BindingFlags.Static), unionType.FullName);
        }

        Assert.IsNotNull(typeof(VuetifyColorValue).GetMethod(
            "op_Implicit",
            BindingFlags.Public | BindingFlags.Static,
            [typeof(decimal)]));
        Assert.IsNotNull(typeof(VuetifyColorPickerSwatches).GetMethod(
            "op_Implicit",
            BindingFlags.Public | BindingFlags.Static,
            [typeof(double[][])]));
    }

    [TestMethod]
    public void VIconBtnValues_UseNativeUnionContracts()
    {
        var unionTypes = new[]
        {
            typeof(VIconBtnSizeMap),
            typeof(VIconBtnTextValue)
        };

        AssertNet11UnionContract(typeof(VIconBtnSizeMap), typeof(VIconBtnSizeEntry[]));
        AssertNet11UnionContract(typeof(VIconBtnTextValue), typeof(bool), typeof(Number), typeof(string));

        foreach (var unionType in unionTypes)
        {
            Assert.IsNull(unionType.GetMethod("From", BindingFlags.Public | BindingFlags.Static), unionType.FullName);
        }

        Assert.IsNotNull(typeof(VIconBtnTextValue).GetMethod(
            "op_Implicit",
            BindingFlags.Public | BindingFlags.Static,
            [typeof(decimal)]));
    }

    [TestMethod]
    public void VSnackbarQueueValues_UseNativeUnionContracts()
    {
        var unionTypes = new[]
        {
            typeof(VuetifySnackbarQueueMessages),
            typeof(VuetifySnackbarQueueMessage)
        };

        AssertNet11UnionContract(typeof(VuetifySnackbarQueueMessages), typeof(VuetifySnackbarQueueMessage[]));
        AssertNet11UnionContract(
            typeof(VuetifySnackbarQueueMessage),
            typeof(string),
            typeof(VuetifySnackbarQueueMessageOptions));

        foreach (var unionType in unionTypes)
        {
            Assert.IsNull(unionType.GetMethod("From", BindingFlags.Public | BindingFlags.Static), unionType.FullName);
        }

        Assert.IsNotNull(typeof(VuetifySnackbarQueueMessages).GetMethod(
            "op_Implicit",
            BindingFlags.Public | BindingFlags.Static,
            [typeof(VuetifySnackbarQueueMessageOptions[])]));
    }

    [TestMethod]
    public void VuetifySlotValues_UseNativeUnionContracts()
    {
        var unionTypes = new[]
        {
            typeof(VuetifyCarouselVerticalDelimiters),
            typeof(VChipSelectedClassValue)
        };

        AssertNet11UnionContract(
            typeof(VuetifyCarouselVerticalDelimiters),
            typeof(bool),
            typeof(VuetifyCarouselVerticalDelimiterPosition));
        AssertNet11UnionContract(typeof(VChipSelectedClassValue), typeof(bool), typeof(string[]));

        foreach (var unionType in unionTypes)
        {
            Assert.IsNull(unionType.GetMethod("From", BindingFlags.Public | BindingFlags.Static), unionType.FullName);
        }
    }

    [TestMethod]
    public void VTimePickerValues_UseNativeUnionContracts()
    {
        var unionTypes = new[]
        {
            typeof(VuetifyTimePickerModelValue),
            typeof(VuetifyTimePickerAllowedUnits),
            typeof(VuetifyTimePickerAllowedUnitValue)
        };

        AssertNet11UnionContract(typeof(VuetifyTimePickerModelValue), typeof(string), typeof(Date));
        AssertNet11UnionContract(typeof(VuetifyTimePickerAllowedUnits), typeof(Number[]));
        AssertNet11UnionContract(
            typeof(VuetifyTimePickerAllowedUnitValue),
            typeof(VuetifyTimePickerAllowedUnits),
            typeof(VuetifyTimePickerAllowedUnitResolver));

        foreach (var unionType in unionTypes)
        {
            Assert.IsNull(unionType.GetMethod("From", BindingFlags.Public | BindingFlags.Static), unionType.FullName);
        }

        Assert.IsNotNull(typeof(VuetifyTimePickerAllowedUnitValue).GetMethod(
            "op_Implicit",
            BindingFlags.Public | BindingFlags.Static,
            [typeof(double[])]));
    }

    [TestMethod]
    public void VConfirmEditValues_UseNativeUnionContracts()
    {
        var unionTypes = new[]
        {
            typeof(VuetifyConfirmEditActions),
            typeof(VuetifyConfirmEditDisabled)
        };

        AssertNet11UnionContract(typeof(VuetifyConfirmEditActions), typeof(VuetifyConfirmEditAction[]));
        AssertNet11UnionContract(typeof(VuetifyConfirmEditDisabled), typeof(bool), typeof(VuetifyConfirmEditActions));

        foreach (var unionType in unionTypes)
        {
            Assert.IsNull(unionType.GetMethod("From", BindingFlags.Public | BindingFlags.Static), unionType.FullName);
        }

        Assert.IsNotNull(typeof(VuetifyConfirmEditDisabled).GetMethod(
            "op_Implicit",
            BindingFlags.Public | BindingFlags.Static,
            [typeof(VuetifyConfirmEditAction[])]));
    }

    [TestMethod]
    public void VDateInputValues_UseNativeUnionContracts()
    {
        AssertNet11UnionContract(
            typeof(VDateInputDisplayFormatValue),
            typeof(string),
            typeof(VDateInputDisplayFormatCallback));
        Assert.IsNull(typeof(VDateInputDisplayFormatValue).GetMethod("From", BindingFlags.Public | BindingFlags.Static));
    }

    [TestMethod]
    public void VDatePickerValues_UseNativeUnionContracts()
    {
        var unionTypes = new[]
        {
            typeof(VuetifyCalendarWeekdays),
            typeof(VuetifyDatePickerMultipleValue),
            typeof(VuetifyDatePickerModelValues),
            typeof(VuetifyDatePickerModelValue),
            typeof(VuetifyDatePickerAllowedDates),
            typeof(VuetifyDatePickerAllowedDatesValue),
            typeof(VuetifyDatePickerActiveValue)
        };

        AssertNet11UnionContract(typeof(VuetifyCalendarWeekdays), typeof(VuetifyCalendarWeekday[]));
        AssertNet11UnionContract(
            typeof(VuetifyDatePickerMultipleValue),
            typeof(bool),
            typeof(Number),
            typeof(VuetifyDatePickerMultipleMode),
            typeof(string));
        AssertNet11UnionContract(typeof(VuetifyDatePickerModelValues), typeof(VueValue[]));
        AssertNet11UnionContract(
            typeof(VuetifyDatePickerModelValue),
            typeof(Date),
            typeof(string),
            typeof(Number),
            typeof(VuetifyDatePickerModelValues));
        AssertNet11UnionContract(typeof(VuetifyDatePickerAllowedDates), typeof(VueValue[]));
        AssertNet11UnionContract(
            typeof(VuetifyDatePickerAllowedDatesValue),
            typeof(VuetifyDatePickerAllowedDates),
            typeof(VuetifyDatePickerAllowedDateResolver));
        AssertNet11UnionContract(typeof(VuetifyDatePickerActiveValue), typeof(string), typeof(string[]));

        foreach (var unionType in unionTypes)
        {
            Assert.IsNull(unionType.GetMethod("From", BindingFlags.Public | BindingFlags.Static), unionType.FullName);
        }

        Assert.IsNotNull(typeof(VuetifyDatePickerMultipleValue).GetMethod(
            "op_Implicit",
            BindingFlags.Public | BindingFlags.Static,
            [typeof(decimal)]));
        Assert.IsNotNull(typeof(VuetifyDatePickerModelValue).GetMethod(
            "op_Implicit",
            BindingFlags.Public | BindingFlags.Static,
            [typeof(double[])]));
        Assert.IsNotNull(typeof(VuetifyDatePickerAllowedDatesValue).GetMethod(
            "op_Implicit",
            BindingFlags.Public | BindingFlags.Static,
            [typeof(double[])]));
    }

    [TestMethod]
    public void TDesign_TaggedUnions_ExposePublicCreationMembers()
        => AssertTaggedUnionsExposePublicCreationMembers(
            typeof(TDesignComponents).Assembly,
            typeof(TDesignComponents).Namespace!);

    private static void AssertTaggedUnionsExposePublicCreationMembers(
        Assembly assembly,
        string contractNamespace)
    {
        var unionTypes = assembly
            .GetTypes()
            .Where(type =>
                type.Namespace == contractNamespace
                && type.IsValueType
                && !type.IsEnum
                && type.GetCustomAttribute<System.Runtime.CompilerServices.UnionAttribute>() is not null
                && typeof(System.Runtime.CompilerServices.IUnion).IsAssignableFrom(type))
            .ToArray();

        Assert.IsTrue(unionTypes.Length > 0, $"Expected tagged union contracts in {contractNamespace}.");

        foreach (var unionType in unionTypes)
        {
            var publicCreationMembers = unionType
                .GetConstructors(BindingFlags.Public | BindingFlags.Instance)
                .Where(static constructor => constructor.GetParameters().Length > 0)
                .ToArray();
            Assert.IsTrue(
                publicCreationMembers.Length > 0,
                $"{unionType.FullName} must expose a public union creation member for C# Preview 6.");

            var privateBranchConstructors = unionType
                .GetConstructors(BindingFlags.NonPublic | BindingFlags.Instance)
                .Where(static constructor => constructor.GetParameters().Length > 0)
                .ToArray();
            Assert.AreEqual(
                0,
                privateBranchConstructors.Length,
                $"{unionType.FullName} has non-public union branch constructors.");
        }
    }

    [TestMethod]
    public void Vuetify_ValueAndUnionTypes_ExposeStronglyTypedContracts()
    {
        Assert.IsNotNull(typeof(VuetifyHideDetailsValue).GetCustomAttribute<ECMAScriptAttribute>());
        Assert.IsNotNull(typeof(VuetifyMessagesValue).GetCustomAttribute<ECMAScriptAttribute>());
        Assert.IsNotNull(typeof(VuetifyScrimValue).GetCustomAttribute<ECMAScriptAttribute>());
        Assert.IsNotNull(typeof(VuetifyDisplayBreakpoint).GetCustomAttribute<ECMAScriptAttribute>());
        CollectionAssert.AreEqual(
            new[] { "elevated", "flat", "outlined", "plain", "text", "tonal" },
            GetStringEnumRuntimeValues(typeof(VuetifyVariant)));
        CollectionAssert.AreEqual(
            new[] { "filled", "outlined", "plain", "solo", "solo-filled", "solo-inverted", "underlined" },
            GetStringEnumRuntimeValues(typeof(VuetifyFieldVariant)));
        CollectionAssert.AreEqual(
            new[] { "color", "date", "datetime-local", "email", "month", "number", "password", "search", "tel", "text", "time", "url", "week" },
            GetStringEnumRuntimeValues(typeof(VuetifyInputType)));
        CollectionAssert.AreEqual(
            new[] { "exact" },
            GetStringEnumRuntimeValues(typeof(VuetifyAutoSelectFirstMode)));
        CollectionAssert.AreEqual(
            new[] { "default", "hidden", "split", "stacked" },
            GetStringEnumRuntimeValues(typeof(VuetifyNumberInputControlVariant)));
        CollectionAssert.AreEqual(
            new[] { "horizontal", "vertical" },
            GetStringEnumRuntimeValues(typeof(VuetifySliderDirection)));
        CollectionAssert.AreEqual(
            new[] { "always" },
            GetStringEnumRuntimeValues(typeof(VuetifyAlwaysMode)));
        CollectionAssert.AreEqual(
            new[] { "disable-shrink" },
            GetStringEnumRuntimeValues(typeof(VuetifyProgressCircularIndeterminateMode)));
        CollectionAssert.AreEqual(
            new[] { "bottom", "top" },
            GetStringEnumRuntimeValues(typeof(VuetifyAppBarLocation)));
        CollectionAssert.AreEqual(
            new[] { "bottom", "end", "left", "right", "start", "top" },
            GetStringEnumRuntimeValues(typeof(VuetifyNavigationDrawerLocation)));

        Assert.AreEqual(typeof(bool?), typeof(VuetifyHideDetailsValue).GetProperty(nameof(VuetifyHideDetailsValue.AsBool), BindingFlags.Public | BindingFlags.Instance)!.PropertyType);
        Assert.AreEqual(typeof(VuetifyHideDetailsMode?), typeof(VuetifyHideDetailsValue).GetProperty(nameof(VuetifyHideDetailsValue.AsMode), BindingFlags.Public | BindingFlags.Instance)!.PropertyType);
        Assert.AreEqual(typeof(VuetifyHideDetailsValue), typeof(VuetifyHideDetailsValue).GetMethod(nameof(VuetifyHideDetailsValue.From), BindingFlags.Public | BindingFlags.Static, [typeof(bool)])!.ReturnType);
        Assert.AreEqual(typeof(VuetifyHideDetailsValue), typeof(VuetifyHideDetailsValue).GetMethod(nameof(VuetifyHideDetailsValue.From), BindingFlags.Public | BindingFlags.Static, [typeof(VuetifyHideDetailsMode)])!.ReturnType);
        Assert.AreEqual("__arg1", typeof(VuetifyHideDetailsValue).GetMethod(nameof(VuetifyHideDetailsValue.From), BindingFlags.Public | BindingFlags.Static, [typeof(bool)])!.GetCustomAttribute<ECMAScriptInlineAttribute>()?.RawFuncCode);
        Assert.AreEqual("__arg1", typeof(VuetifyHideDetailsValue).GetMethod(nameof(VuetifyHideDetailsValue.From), BindingFlags.Public | BindingFlags.Static, [typeof(VuetifyHideDetailsMode)])!.GetCustomAttribute<ECMAScriptInlineAttribute>()?.RawFuncCode);

        Assert.AreEqual(typeof(string), typeof(VuetifyMessagesValue).GetProperty(nameof(VuetifyMessagesValue.AsString), BindingFlags.Public | BindingFlags.Instance)!.PropertyType);
        Assert.AreEqual(typeof(string[]), typeof(VuetifyMessagesValue).GetProperty(nameof(VuetifyMessagesValue.AsStrings), BindingFlags.Public | BindingFlags.Instance)!.PropertyType);
        Assert.AreEqual(typeof(VuetifyMessagesValue), typeof(VuetifyMessagesValue).GetMethod(nameof(VuetifyMessagesValue.From), BindingFlags.Public | BindingFlags.Static, [typeof(string)])!.ReturnType);
        Assert.AreEqual(typeof(VuetifyMessagesValue), typeof(VuetifyMessagesValue).GetMethod(nameof(VuetifyMessagesValue.From), BindingFlags.Public | BindingFlags.Static, [typeof(string[])])!.ReturnType);
        Assert.AreEqual("__arg1", typeof(VuetifyMessagesValue).GetMethod(nameof(VuetifyMessagesValue.From), BindingFlags.Public | BindingFlags.Static, [typeof(string)])!.GetCustomAttribute<ECMAScriptInlineAttribute>()?.RawFuncCode);
        Assert.AreEqual("__arg1", typeof(VuetifyMessagesValue).GetMethod(nameof(VuetifyMessagesValue.From), BindingFlags.Public | BindingFlags.Static, [typeof(string[])])!.GetCustomAttribute<ECMAScriptInlineAttribute>()?.RawFuncCode);
        Assert.AreEqual(typeof(bool), typeof(VuetifyScrimValue).GetProperty(nameof(VuetifyScrimValue.AsBool), BindingFlags.Public | BindingFlags.Instance)!.PropertyType.UnwrapNullable());
        Assert.AreEqual(typeof(string), typeof(VuetifyScrimValue).GetProperty(nameof(VuetifyScrimValue.AsString), BindingFlags.Public | BindingFlags.Instance)!.PropertyType.UnwrapNullable());
        Assert.AreEqual(typeof(VuetifyScrimValue), typeof(VuetifyScrimValue).GetMethod(nameof(VuetifyScrimValue.From), BindingFlags.Public | BindingFlags.Static, [typeof(bool)])!.ReturnType);
        Assert.AreEqual(typeof(VuetifyScrimValue), typeof(VuetifyScrimValue).GetMethod(nameof(VuetifyScrimValue.From), BindingFlags.Public | BindingFlags.Static, [typeof(string)])!.ReturnType);
        Assert.AreEqual("__arg1", typeof(VuetifyScrimValue).GetMethod(nameof(VuetifyScrimValue.From), BindingFlags.Public | BindingFlags.Static, [typeof(bool)])!.GetCustomAttribute<ECMAScriptInlineAttribute>()?.RawFuncCode);
        Assert.AreEqual("__arg1", typeof(VuetifyScrimValue).GetMethod(nameof(VuetifyScrimValue.From), BindingFlags.Public | BindingFlags.Static, [typeof(string)])!.GetCustomAttribute<ECMAScriptInlineAttribute>()?.RawFuncCode);
        Assert.AreEqual(typeof(string), typeof(VuetifyDisplayBreakpoint).GetProperty(nameof(VuetifyDisplayBreakpoint.AsString), BindingFlags.Public | BindingFlags.Instance)!.PropertyType.UnwrapNullable());
        Assert.AreEqual(typeof(Number), typeof(VuetifyDisplayBreakpoint).GetProperty(nameof(VuetifyDisplayBreakpoint.AsNumber), BindingFlags.Public | BindingFlags.Instance)!.PropertyType.UnwrapNullable());
        Assert.AreEqual(typeof(VuetifyDisplayBreakpoint), typeof(VuetifyDisplayBreakpoint).GetMethod(nameof(VuetifyDisplayBreakpoint.From), BindingFlags.Public | BindingFlags.Static, [typeof(string)])!.ReturnType);
        Assert.AreEqual(typeof(VuetifyDisplayBreakpoint), typeof(VuetifyDisplayBreakpoint).GetMethod(nameof(VuetifyDisplayBreakpoint.From), BindingFlags.Public | BindingFlags.Static, [typeof(Number)])!.ReturnType);
        Assert.AreEqual("__arg1", typeof(VuetifyDisplayBreakpoint).GetMethod(nameof(VuetifyDisplayBreakpoint.From), BindingFlags.Public | BindingFlags.Static, [typeof(string)])!.GetCustomAttribute<ECMAScriptInlineAttribute>()?.RawFuncCode);
        Assert.AreEqual("__arg1", typeof(VuetifyDisplayBreakpoint).GetMethod(nameof(VuetifyDisplayBreakpoint.From), BindingFlags.Public | BindingFlags.Static, [typeof(Number)])!.GetCustomAttribute<ECMAScriptInlineAttribute>()?.RawFuncCode);

        Assert.AreEqual(typeof(bool), typeof(VuetifyAutoSelectFirstValue).GetProperty(nameof(VuetifyAutoSelectFirstValue.AsBool), BindingFlags.Public | BindingFlags.Instance)!.PropertyType.UnwrapNullable());
        Assert.AreEqual(typeof(VuetifyAutoSelectFirstMode), typeof(VuetifyAutoSelectFirstValue).GetProperty(nameof(VuetifyAutoSelectFirstValue.AsMode), BindingFlags.Public | BindingFlags.Instance)!.PropertyType.UnwrapNullable());
        Assert.AreEqual(typeof(VuetifyAutoSelectFirstValue), typeof(VuetifyAutoSelectFirstValue).GetMethod(nameof(VuetifyAutoSelectFirstValue.From), BindingFlags.Public | BindingFlags.Static, [typeof(bool)])!.ReturnType);
        Assert.AreEqual(typeof(VuetifyAutoSelectFirstValue), typeof(VuetifyAutoSelectFirstValue).GetMethod(nameof(VuetifyAutoSelectFirstValue.From), BindingFlags.Public | BindingFlags.Static, [typeof(VuetifyAutoSelectFirstMode)])!.ReturnType);
        Assert.AreEqual("__arg1", typeof(VuetifyAutoSelectFirstValue).GetMethod(nameof(VuetifyAutoSelectFirstValue.From), BindingFlags.Public | BindingFlags.Static, [typeof(VuetifyAutoSelectFirstMode)])!.GetCustomAttribute<ECMAScriptInlineAttribute>()?.RawFuncCode);
        Assert.AreEqual(typeof(bool), typeof(VuetifyFileShowSizeValue).GetProperty(nameof(VuetifyFileShowSizeValue.AsBool), BindingFlags.Public | BindingFlags.Instance)!.PropertyType.UnwrapNullable());
        Assert.AreEqual(typeof(VuetifyFileSizeBase), typeof(VuetifyFileShowSizeValue).GetProperty(nameof(VuetifyFileShowSizeValue.AsBase), BindingFlags.Public | BindingFlags.Instance)!.PropertyType.UnwrapNullable());
        Assert.AreEqual(typeof(VuetifyFileShowSizeValue), typeof(VuetifyFileShowSizeValue).GetMethod(nameof(VuetifyFileShowSizeValue.From), BindingFlags.Public | BindingFlags.Static, [typeof(bool)])!.ReturnType);
        Assert.AreEqual(typeof(VuetifyFileShowSizeValue), typeof(VuetifyFileShowSizeValue).GetMethod(nameof(VuetifyFileShowSizeValue.From), BindingFlags.Public | BindingFlags.Static, [typeof(VuetifyFileSizeBase)])!.ReturnType);
        Assert.AreEqual("__arg1", typeof(VuetifyFileShowSizeValue).GetMethod(nameof(VuetifyFileShowSizeValue.From), BindingFlags.Public | BindingFlags.Static, [typeof(VuetifyFileSizeBase)])!.GetCustomAttribute<ECMAScriptInlineAttribute>()?.RawFuncCode);
        Assert.AreEqual(typeof(bool), typeof(VuetifyBooleanAlwaysValue).GetProperty(nameof(VuetifyBooleanAlwaysValue.AsBool), BindingFlags.Public | BindingFlags.Instance)!.PropertyType.UnwrapNullable());
        Assert.AreEqual(typeof(VuetifyAlwaysMode), typeof(VuetifyBooleanAlwaysValue).GetProperty(nameof(VuetifyBooleanAlwaysValue.AsMode), BindingFlags.Public | BindingFlags.Instance)!.PropertyType.UnwrapNullable());
        Assert.AreEqual(typeof(VuetifyBooleanAlwaysValue), typeof(VuetifyBooleanAlwaysValue).GetMethod(nameof(VuetifyBooleanAlwaysValue.From), BindingFlags.Public | BindingFlags.Static, [typeof(bool)])!.ReturnType);
        Assert.AreEqual(typeof(VuetifyBooleanAlwaysValue), typeof(VuetifyBooleanAlwaysValue).GetMethod(nameof(VuetifyBooleanAlwaysValue.From), BindingFlags.Public | BindingFlags.Static, [typeof(VuetifyAlwaysMode)])!.ReturnType);
        Assert.AreEqual(typeof(bool), typeof(VuetifyBooleanStringValue).GetProperty(nameof(VuetifyBooleanStringValue.AsBool), BindingFlags.Public | BindingFlags.Instance)!.PropertyType.UnwrapNullable());
        Assert.AreEqual(typeof(string), typeof(VuetifyBooleanStringValue).GetProperty(nameof(VuetifyBooleanStringValue.AsString), BindingFlags.Public | BindingFlags.Instance)!.PropertyType.UnwrapNullable());
        Assert.AreEqual(typeof(VuetifyBooleanStringValue), typeof(VuetifyBooleanStringValue).GetMethod(nameof(VuetifyBooleanStringValue.From), BindingFlags.Public | BindingFlags.Static, [typeof(bool)])!.ReturnType);
        Assert.AreEqual(typeof(VuetifyBooleanStringValue), typeof(VuetifyBooleanStringValue).GetMethod(nameof(VuetifyBooleanStringValue.From), BindingFlags.Public | BindingFlags.Static, [typeof(string)])!.ReturnType);
        Assert.AreEqual("__arg1", typeof(VuetifyBooleanStringValue).GetMethod(nameof(VuetifyBooleanStringValue.From), BindingFlags.Public | BindingFlags.Static, [typeof(string)])!.GetCustomAttribute<ECMAScriptInlineAttribute>()?.RawFuncCode);
        Assert.AreEqual(typeof(bool), typeof(VuetifyCounterValue).GetProperty(nameof(VuetifyCounterValue.AsBool), BindingFlags.Public | BindingFlags.Instance)!.PropertyType.UnwrapNullable());
        Assert.AreEqual(typeof(Number), typeof(VuetifyCounterValue).GetProperty(nameof(VuetifyCounterValue.AsNumber), BindingFlags.Public | BindingFlags.Instance)!.PropertyType.UnwrapNullable());
        Assert.AreEqual(typeof(string), typeof(VuetifyCounterValue).GetProperty(nameof(VuetifyCounterValue.AsString), BindingFlags.Public | BindingFlags.Instance)!.PropertyType.UnwrapNullable());
        Assert.AreEqual(typeof(VuetifyCounterValue), typeof(VuetifyCounterValue).GetMethod(nameof(VuetifyCounterValue.From), BindingFlags.Public | BindingFlags.Static, [typeof(bool)])!.ReturnType);
        Assert.AreEqual(typeof(VuetifyCounterValue), typeof(VuetifyCounterValue).GetMethod(nameof(VuetifyCounterValue.From), BindingFlags.Public | BindingFlags.Static, [typeof(Number)])!.ReturnType);
        Assert.AreEqual(typeof(VuetifyCounterValue), typeof(VuetifyCounterValue).GetMethod(nameof(VuetifyCounterValue.From), BindingFlags.Public | BindingFlags.Static, [typeof(string)])!.ReturnType);
        Assert.AreEqual("__arg1", typeof(VuetifyCounterValue).GetMethod(nameof(VuetifyCounterValue.From), BindingFlags.Public | BindingFlags.Static, [typeof(Number)])!.GetCustomAttribute<ECMAScriptInlineAttribute>()?.RawFuncCode);
        Assert.AreEqual(typeof(string), typeof(VuetifyTextValue).GetProperty(nameof(VuetifyTextValue.AsString), BindingFlags.Public | BindingFlags.Instance)!.PropertyType.UnwrapNullable());
        Assert.AreEqual(typeof(Number), typeof(VuetifyTextValue).GetProperty(nameof(VuetifyTextValue.AsNumber), BindingFlags.Public | BindingFlags.Instance)!.PropertyType.UnwrapNullable());
        Assert.AreEqual(typeof(bool), typeof(VuetifyTextValue).GetProperty(nameof(VuetifyTextValue.AsBool), BindingFlags.Public | BindingFlags.Instance)!.PropertyType.UnwrapNullable());
        Assert.AreEqual(typeof(VuetifyTextValue), typeof(VuetifyTextValue).GetMethod(nameof(VuetifyTextValue.From), BindingFlags.Public | BindingFlags.Static, [typeof(bool)])!.ReturnType);
        Assert.AreEqual(typeof(bool), typeof(VuetifyRoundedValue).GetProperty(nameof(VuetifyRoundedValue.AsBool), BindingFlags.Public | BindingFlags.Instance)!.PropertyType.UnwrapNullable());
        Assert.AreEqual(typeof(Number), typeof(VuetifyRoundedValue).GetProperty(nameof(VuetifyRoundedValue.AsNumber), BindingFlags.Public | BindingFlags.Instance)!.PropertyType.UnwrapNullable());
        Assert.AreEqual(typeof(string), typeof(VuetifyRoundedValue).GetProperty(nameof(VuetifyRoundedValue.AsString), BindingFlags.Public | BindingFlags.Instance)!.PropertyType.UnwrapNullable());
        Assert.AreEqual(typeof(VuetifyRoundedValue), typeof(VuetifyRoundedValue).GetMethod(nameof(VuetifyRoundedValue.From), BindingFlags.Public | BindingFlags.Static, [typeof(bool)])!.ReturnType);
        Assert.AreEqual(typeof(VuetifyRoundedValue), typeof(VuetifyRoundedValue).GetMethod(nameof(VuetifyRoundedValue.From), BindingFlags.Public | BindingFlags.Static, [typeof(Number)])!.ReturnType);
        Assert.AreEqual(typeof(bool), typeof(VuetifyProgressCircularIndeterminateValue).GetProperty(nameof(VuetifyProgressCircularIndeterminateValue.AsBool), BindingFlags.Public | BindingFlags.Instance)!.PropertyType.UnwrapNullable());
        Assert.AreEqual(typeof(VuetifyProgressCircularIndeterminateMode), typeof(VuetifyProgressCircularIndeterminateValue).GetProperty(nameof(VuetifyProgressCircularIndeterminateValue.AsMode), BindingFlags.Public | BindingFlags.Instance)!.PropertyType.UnwrapNullable());
        Assert.AreEqual(typeof(VuetifyProgressCircularIndeterminateValue), typeof(VuetifyProgressCircularIndeterminateValue).GetMethod(nameof(VuetifyProgressCircularIndeterminateValue.From), BindingFlags.Public | BindingFlags.Static, [typeof(bool)])!.ReturnType);
        Assert.AreEqual(typeof(VuetifyProgressCircularIndeterminateValue), typeof(VuetifyProgressCircularIndeterminateValue).GetMethod(nameof(VuetifyProgressCircularIndeterminateValue.From), BindingFlags.Public | BindingFlags.Static, [typeof(VuetifyProgressCircularIndeterminateMode)])!.ReturnType);
        Assert.AreEqual(typeof(ECMAScript.File), typeof(VuetifyFileModelValue).GetProperty(nameof(VuetifyFileModelValue.AsFile), BindingFlags.Public | BindingFlags.Instance)!.PropertyType.UnwrapNullable());
        Assert.AreEqual(typeof(ECMAScript.File[]), typeof(VuetifyFileModelValue).GetProperty(nameof(VuetifyFileModelValue.AsFiles), BindingFlags.Public | BindingFlags.Instance)!.PropertyType);
        Assert.AreEqual(typeof(VuetifyFileModelValue), typeof(VuetifyFileModelValue).GetMethod(nameof(VuetifyFileModelValue.From), BindingFlags.Public | BindingFlags.Static, [typeof(ECMAScript.File)])!.ReturnType);
        Assert.AreEqual(typeof(VuetifyFileModelValue), typeof(VuetifyFileModelValue).GetMethod(nameof(VuetifyFileModelValue.From), BindingFlags.Public | BindingFlags.Static, [typeof(ECMAScript.File[])])!.ReturnType);
        Assert.AreEqual(typeof(Number[]), typeof(VuetifyRangeSliderModelValue).GetProperty(nameof(VuetifyRangeSliderModelValue.AsArray), BindingFlags.Public | BindingFlags.Instance)!.PropertyType);
        Assert.AreEqual(typeof(VuetifyRangeSliderModelValue), typeof(VuetifyRangeSliderModelValue).GetMethod(nameof(VuetifyRangeSliderModelValue.From), BindingFlags.Public | BindingFlags.Static, [typeof(Number[])])!.ReturnType);
        Assert.AreEqual(typeof(VuetifyRangeSliderModelValue), typeof(VuetifyRangeSliderModelValue).GetMethod(nameof(VuetifyRangeSliderModelValue.From), BindingFlags.Public | BindingFlags.Static, [typeof(Number), typeof(Number)])!.ReturnType);
        Assert.AreEqual("[__arg1, __arg2]", typeof(VuetifyRangeSliderModelValue).GetMethod(nameof(VuetifyRangeSliderModelValue.From), BindingFlags.Public | BindingFlags.Static, [typeof(Number), typeof(Number)])!.GetCustomAttribute<ECMAScriptInlineAttribute>()?.RawFuncCode);

        var selectModelValuesCollectionBuilder = typeof(VuetifySelectModelValues).GetCustomAttribute<System.Runtime.CompilerServices.CollectionBuilderAttribute>();
        Assert.IsNotNull(selectModelValuesCollectionBuilder);
        Assert.AreEqual(typeof(VuetifySelectModelValuesCollectionBuilder), selectModelValuesCollectionBuilder!.BuilderType);
        Assert.AreEqual(nameof(VuetifySelectModelValuesCollectionBuilder.Create), selectModelValuesCollectionBuilder.MethodName);
        Assert.AreEqual(typeof(VuetifySelectModelValue[]), typeof(VuetifySelectModelValues).GetProperty(nameof(VuetifySelectModelValues.AsArray), BindingFlags.Public | BindingFlags.Instance)!.PropertyType.UnwrapNullable());
        Assert.AreEqual(typeof(VuetifySelectModelValues), typeof(VuetifySelectModelValues).GetMethod(nameof(VuetifySelectModelValues.From), BindingFlags.Public | BindingFlags.Static, [typeof(VuetifySelectModelValue[])])!.ReturnType);
        Assert.AreEqual("__arg1", typeof(VuetifySelectModelValues).GetMethod(nameof(VuetifySelectModelValues.From), BindingFlags.Public | BindingFlags.Static, [typeof(VuetifySelectModelValue[])])!.GetCustomAttribute<ECMAScriptInlineAttribute>()?.RawFuncCode);
        Assert.AreEqual(typeof(string), typeof(VuetifySelectModelValue).GetProperty(nameof(VuetifySelectModelValue.AsString), BindingFlags.Public | BindingFlags.Instance)!.PropertyType.UnwrapNullable());
        Assert.AreEqual(typeof(Number), typeof(VuetifySelectModelValue).GetProperty(nameof(VuetifySelectModelValue.AsNumber), BindingFlags.Public | BindingFlags.Instance)!.PropertyType.UnwrapNullable());
        Assert.AreEqual(typeof(bool), typeof(VuetifySelectModelValue).GetProperty(nameof(VuetifySelectModelValue.AsBool), BindingFlags.Public | BindingFlags.Instance)!.PropertyType.UnwrapNullable());
        Assert.AreEqual(typeof(Symbol), typeof(VuetifySelectModelValue).GetProperty(nameof(VuetifySelectModelValue.AsSymbol), BindingFlags.Public | BindingFlags.Instance)!.PropertyType.UnwrapNullable());
        Assert.AreEqual(typeof(VueProps), typeof(VuetifySelectModelValue).GetProperty(nameof(VuetifySelectModelValue.AsObject), BindingFlags.Public | BindingFlags.Instance)!.PropertyType.UnwrapNullable());
        Assert.AreEqual(typeof(VuetifySelectModelValues), typeof(VuetifySelectModelValue).GetProperty(nameof(VuetifySelectModelValue.AsValues), BindingFlags.Public | BindingFlags.Instance)!.PropertyType.UnwrapNullable());
        Assert.AreEqual(typeof(VuetifySelectModelValue), typeof(VuetifySelectModelValue).GetMethod(nameof(VuetifySelectModelValue.From), BindingFlags.Public | BindingFlags.Static, [typeof(string)])!.ReturnType);
        Assert.AreEqual(typeof(VuetifySelectModelValue), typeof(VuetifySelectModelValue).GetMethod(nameof(VuetifySelectModelValue.From), BindingFlags.Public | BindingFlags.Static, [typeof(Number)])!.ReturnType);
        Assert.AreEqual(typeof(VuetifySelectModelValue), typeof(VuetifySelectModelValue).GetMethod(nameof(VuetifySelectModelValue.From), BindingFlags.Public | BindingFlags.Static, [typeof(bool)])!.ReturnType);
        Assert.AreEqual(typeof(VuetifySelectModelValue), typeof(VuetifySelectModelValue).GetMethod(nameof(VuetifySelectModelValue.From), BindingFlags.Public | BindingFlags.Static, [typeof(Symbol)])!.ReturnType);
        Assert.AreEqual(typeof(VuetifySelectModelValue), typeof(VuetifySelectModelValue).GetMethod(nameof(VuetifySelectModelValue.From), BindingFlags.Public | BindingFlags.Static, [typeof(VueProps)])!.ReturnType);
        Assert.AreEqual(typeof(VuetifySelectModelValue), typeof(VuetifySelectModelValue).GetMethod(nameof(VuetifySelectModelValue.From), BindingFlags.Public | BindingFlags.Static, [typeof(VuetifySelectModelValues)])!.ReturnType);
        Assert.AreEqual("__arg1", typeof(VuetifySelectModelValue).GetMethod(nameof(VuetifySelectModelValue.From), BindingFlags.Public | BindingFlags.Static, [typeof(Number)])!.GetCustomAttribute<ECMAScriptInlineAttribute>()?.RawFuncCode);
        Assert.AreEqual("__arg1", typeof(VuetifySelectModelValue).GetMethod(nameof(VuetifySelectModelValue.From), BindingFlags.Public | BindingFlags.Static, [typeof(Symbol)])!.GetCustomAttribute<ECMAScriptInlineAttribute>()?.RawFuncCode);
        Assert.AreEqual("__arg1", typeof(VuetifySelectModelValue).GetMethod(nameof(VuetifySelectModelValue.From), BindingFlags.Public | BindingFlags.Static, [typeof(VuetifySelectModelValues)])!.GetCustomAttribute<ECMAScriptInlineAttribute>()?.RawFuncCode);
        Assert.AreEqual(typeof(string), typeof(VuetifySelectItemValue).GetProperty(nameof(VuetifySelectItemValue.AsString), BindingFlags.Public | BindingFlags.Instance)!.PropertyType.UnwrapNullable());
        Assert.AreEqual(typeof(VuetifySelectItem), typeof(VuetifySelectItemValue).GetProperty(nameof(VuetifySelectItemValue.AsItem), BindingFlags.Public | BindingFlags.Instance)!.PropertyType.UnwrapNullable());
        Assert.AreEqual(typeof(Number), typeof(VuetifySelectItemValue).GetProperty(nameof(VuetifySelectItemValue.AsNumber), BindingFlags.Public | BindingFlags.Instance)!.PropertyType.UnwrapNullable());
        Assert.AreEqual(typeof(bool), typeof(VuetifySelectItemValue).GetProperty(nameof(VuetifySelectItemValue.AsBool), BindingFlags.Public | BindingFlags.Instance)!.PropertyType.UnwrapNullable());
        Assert.AreEqual(typeof(VuetifySelectItemValue), typeof(VuetifySelectItemValue).GetMethod(nameof(VuetifySelectItemValue.From), BindingFlags.Public | BindingFlags.Static, [typeof(string)])!.ReturnType);
        Assert.AreEqual(typeof(VuetifySelectItemValue), typeof(VuetifySelectItemValue).GetMethod(nameof(VuetifySelectItemValue.From), BindingFlags.Public | BindingFlags.Static, [typeof(VuetifySelectItem)])!.ReturnType);
        Assert.AreEqual(typeof(VuetifySelectItemValue), typeof(VuetifySelectItemValue).GetMethod(nameof(VuetifySelectItemValue.From), BindingFlags.Public | BindingFlags.Static, [typeof(Number)])!.ReturnType);
        Assert.AreEqual(typeof(VuetifySelectItemValue), typeof(VuetifySelectItemValue).GetMethod(nameof(VuetifySelectItemValue.From), BindingFlags.Public | BindingFlags.Static, [typeof(bool)])!.ReturnType);
        Assert.AreEqual("__arg1", typeof(VuetifySelectItemValue).GetMethod(nameof(VuetifySelectItemValue.From), BindingFlags.Public | BindingFlags.Static, [typeof(Number)])!.GetCustomAttribute<ECMAScriptInlineAttribute>()?.RawFuncCode);
        Assert.AreEqual("__arg1", typeof(VuetifySelectItemValue).GetMethod(nameof(VuetifySelectItemValue.From), BindingFlags.Public | BindingFlags.Static, [typeof(bool)])!.GetCustomAttribute<ECMAScriptInlineAttribute>()?.RawFuncCode);
        Assert.AreEqual(typeof(string), typeof(VuetifyBreadcrumbItemValue).GetProperty(nameof(VuetifyBreadcrumbItemValue.AsString), BindingFlags.Public | BindingFlags.Instance)!.PropertyType.UnwrapNullable());
        Assert.AreEqual(typeof(VuetifyBreadcrumbItem), typeof(VuetifyBreadcrumbItemValue).GetProperty(nameof(VuetifyBreadcrumbItemValue.AsItem), BindingFlags.Public | BindingFlags.Instance)!.PropertyType.UnwrapNullable());
        Assert.AreEqual(typeof(Number), typeof(VuetifyBreadcrumbItemValue).GetProperty(nameof(VuetifyBreadcrumbItemValue.AsNumber), BindingFlags.Public | BindingFlags.Instance)!.PropertyType.UnwrapNullable());
        Assert.AreEqual(typeof(VuetifyBreadcrumbItemValue), typeof(VuetifyBreadcrumbItemValue).GetMethod(nameof(VuetifyBreadcrumbItemValue.From), BindingFlags.Public | BindingFlags.Static, [typeof(string)])!.ReturnType);
        Assert.AreEqual(typeof(VuetifyBreadcrumbItemValue), typeof(VuetifyBreadcrumbItemValue).GetMethod(nameof(VuetifyBreadcrumbItemValue.From), BindingFlags.Public | BindingFlags.Static, [typeof(VuetifyBreadcrumbItem)])!.ReturnType);
        Assert.AreEqual(typeof(VuetifyBreadcrumbItemValue), typeof(VuetifyBreadcrumbItemValue).GetMethod(nameof(VuetifyBreadcrumbItemValue.From), BindingFlags.Public | BindingFlags.Static, [typeof(Number)])!.ReturnType);
        Assert.AreEqual("__arg1", typeof(VuetifyBreadcrumbItemValue).GetMethod(nameof(VuetifyBreadcrumbItemValue.From), BindingFlags.Public | BindingFlags.Static, [typeof(Number)])!.GetCustomAttribute<ECMAScriptInlineAttribute>()?.RawFuncCode);
        Assert.AreEqual(typeof(VuetifySelectItems), typeof(VSelect).GetProperty(nameof(VSelect.Items), BindingFlags.Public | BindingFlags.Instance)!.PropertyType.UnwrapNullable());
        Assert.AreEqual(typeof(VuetifySelectItemKey), typeof(VSelect).GetProperty(nameof(VSelect.ItemTitle), BindingFlags.Public | BindingFlags.Instance)!.PropertyType.UnwrapNullable());
        Assert.AreEqual(typeof(VuetifySelectItemKey), typeof(VSelect).GetProperty(nameof(VSelect.ItemValue), BindingFlags.Public | BindingFlags.Instance)!.PropertyType.UnwrapNullable());
        Assert.AreEqual(typeof(VuetifySelectItemPropsSelector), typeof(VSelect).GetProperty(nameof(VSelect.ItemProps), BindingFlags.Public | BindingFlags.Instance)!.PropertyType.UnwrapNullable());
        Assert.AreEqual(typeof(VuetifySelectModelValue), typeof(VSelect).GetProperty(nameof(VSelect.SelectedValue), BindingFlags.Public | BindingFlags.Instance)!.PropertyType.UnwrapNullable());
        Assert.AreEqual(typeof(EventCallback<VuetifySelectModelValue?>), typeof(VSelect).GetProperty(nameof(VSelect.SelectedValueChanged), BindingFlags.Public | BindingFlags.Instance)!.PropertyType);
        Assert.AreEqual(typeof(VuetifySelectItems), typeof(VAutocomplete).GetProperty(nameof(VAutocomplete.Items), BindingFlags.Public | BindingFlags.Instance)!.PropertyType.UnwrapNullable());
        Assert.AreEqual(typeof(VuetifySelectItemKey), typeof(VAutocomplete).GetProperty(nameof(VAutocomplete.ItemTitle), BindingFlags.Public | BindingFlags.Instance)!.PropertyType.UnwrapNullable());
        Assert.AreEqual(typeof(VuetifySelectItemKey), typeof(VAutocomplete).GetProperty(nameof(VAutocomplete.ItemValue), BindingFlags.Public | BindingFlags.Instance)!.PropertyType.UnwrapNullable());
        Assert.AreEqual(typeof(VuetifySelectItemPropsSelector), typeof(VAutocomplete).GetProperty(nameof(VAutocomplete.ItemProps), BindingFlags.Public | BindingFlags.Instance)!.PropertyType.UnwrapNullable());
        Assert.AreEqual(typeof(VuetifySelectModelValue), typeof(VAutocomplete).GetProperty(nameof(VAutocomplete.SelectedValue), BindingFlags.Public | BindingFlags.Instance)!.PropertyType.UnwrapNullable());
        Assert.AreEqual(typeof(EventCallback<VuetifySelectModelValue?>), typeof(VAutocomplete).GetProperty(nameof(VAutocomplete.SelectedValueChanged), BindingFlags.Public | BindingFlags.Instance)!.PropertyType);
        Assert.AreEqual(typeof(VuetifyBooleanStringValue), typeof(VBtn).GetProperty(nameof(VBtn.Loading), BindingFlags.Public | BindingFlags.Instance)!.PropertyType.UnwrapNullable());
        Assert.AreEqual(typeof(VuetifyTextValue), typeof(VBtn).GetProperty(nameof(VBtn.Text), BindingFlags.Public | BindingFlags.Instance)!.PropertyType.UnwrapNullable());
        Assert.AreEqual(typeof(VueStringNumberValue), typeof(VBtn).GetProperty(nameof(VBtn.Size), BindingFlags.Public | BindingFlags.Instance)!.PropertyType.UnwrapNullable());
        Assert.AreEqual(typeof(VueStringNumberValue), typeof(VBtn).GetProperty(nameof(VBtn.Height), BindingFlags.Public | BindingFlags.Instance)!.PropertyType.UnwrapNullable());
        Assert.AreEqual(typeof(VuetifyRoundedValue), typeof(VBtn).GetProperty(nameof(VBtn.Rounded), BindingFlags.Public | BindingFlags.Instance)!.PropertyType.UnwrapNullable());
        Assert.AreEqual(typeof(VuetifyCounterValue), typeof(VTextField).GetProperty(nameof(VTextField.Counter), BindingFlags.Public | BindingFlags.Instance)!.PropertyType.UnwrapNullable());
        Assert.AreEqual(typeof(VuetifyCounterValue), typeof(VTextarea).GetProperty(nameof(VTextarea.Counter), BindingFlags.Public | BindingFlags.Instance)!.PropertyType.UnwrapNullable());
        Assert.AreEqual(typeof(VuetifyBooleanStringValue), typeof(VSwitch).GetProperty(nameof(VSwitch.Loading), BindingFlags.Public | BindingFlags.Instance)!.PropertyType.UnwrapNullable());
        Assert.AreEqual(typeof(VueStringNumberValue), typeof(VAvatar).GetProperty(nameof(VAvatar.Size), BindingFlags.Public | BindingFlags.Instance)!.PropertyType.UnwrapNullable());
        Assert.AreEqual(typeof(VuetifyRoundedValue), typeof(VAvatar).GetProperty(nameof(VAvatar.Rounded), BindingFlags.Public | BindingFlags.Instance)!.PropertyType.UnwrapNullable());
        Assert.AreEqual(typeof(VueStringNumberValue), typeof(VImg).GetProperty(nameof(VImg.Width), BindingFlags.Public | BindingFlags.Instance)!.PropertyType.UnwrapNullable());
        Assert.AreEqual(typeof(VuetifyRoundedValue), typeof(VImg).GetProperty(nameof(VImg.Rounded), BindingFlags.Public | BindingFlags.Instance)!.PropertyType.UnwrapNullable());
        Assert.AreEqual(typeof(VueStringNumberValue), typeof(VSheet).GetProperty(nameof(VSheet.Elevation), BindingFlags.Public | BindingFlags.Instance)!.PropertyType.UnwrapNullable());
        Assert.AreEqual(typeof(VuetifyRoundedValue), typeof(VSheet).GetProperty(nameof(VSheet.Rounded), BindingFlags.Public | BindingFlags.Instance)!.PropertyType.UnwrapNullable());
        Assert.AreEqual(typeof(VueStringNumberValue), typeof(VProgressLinear).GetProperty(nameof(VProgressLinear.ModelValue), BindingFlags.Public | BindingFlags.Instance)!.PropertyType.UnwrapNullable());
        Assert.AreEqual(typeof(VuetifyRoundedValue), typeof(VProgressLinear).GetProperty(nameof(VProgressLinear.Rounded), BindingFlags.Public | BindingFlags.Instance)!.PropertyType.UnwrapNullable());
        Assert.AreEqual(typeof(VueStringNumberValue), typeof(VProgressCircular).GetProperty(nameof(VProgressCircular.ModelValue), BindingFlags.Public | BindingFlags.Instance)!.PropertyType.UnwrapNullable());
        Assert.AreEqual(typeof(VuetifyProgressCircularIndeterminateValue), typeof(VProgressCircular).GetProperty(nameof(VProgressCircular.Indeterminate), BindingFlags.Public | BindingFlags.Instance)!.PropertyType.UnwrapNullable());
        Assert.AreEqual(typeof(VueStringNumberValue), typeof(VOtpInput).GetProperty(nameof(VOtpInput.Length), BindingFlags.Public | BindingFlags.Instance)!.PropertyType.UnwrapNullable());
        Assert.AreEqual(typeof(VuetifyBooleanStringValue), typeof(VOtpInput).GetProperty(nameof(VOtpInput.Loading), BindingFlags.Public | BindingFlags.Instance)!.PropertyType.UnwrapNullable());
        Assert.AreEqual(typeof(VueStringNumberValue), typeof(VPagination).GetProperty(nameof(VPagination.Length), BindingFlags.Public | BindingFlags.Instance)!.PropertyType.UnwrapNullable());
        Assert.AreEqual(typeof(VueStringNumberValue), typeof(VPagination).GetProperty(nameof(VPagination.TotalVisible), BindingFlags.Public | BindingFlags.Instance)!.PropertyType.UnwrapNullable());
        Assert.AreEqual(typeof(VueStringNumberValue), typeof(VTextarea).GetProperty(nameof(VTextarea.Rows), BindingFlags.Public | BindingFlags.Instance)!.PropertyType.UnwrapNullable());
        Assert.AreEqual(typeof(VueStringNumberValue), typeof(VTextarea).GetProperty(nameof(VTextarea.MaxRows), BindingFlags.Public | BindingFlags.Instance)!.PropertyType.UnwrapNullable());
        Assert.AreEqual(typeof(VueStringNumberValue), typeof(VBadge).GetProperty(nameof(VBadge.Content), BindingFlags.Public | BindingFlags.Instance)!.PropertyType.UnwrapNullable());
        Assert.AreEqual(typeof(VuetifyTextValue), typeof(VChip).GetProperty(nameof(VChip.Text), BindingFlags.Public | BindingFlags.Instance)!.PropertyType.UnwrapNullable());
        Assert.AreEqual(typeof(VueStringNumberValue), typeof(VChip).GetProperty(nameof(VChip.Size), BindingFlags.Public | BindingFlags.Instance)!.PropertyType.UnwrapNullable());
        Assert.AreEqual(typeof(VuetifyBooleanStringValue), typeof(VSnackbar).GetProperty(nameof(VSnackbar.Timer), BindingFlags.Public | BindingFlags.Instance)!.PropertyType.UnwrapNullable());
        Assert.AreEqual(typeof(VueStringNumberValue), typeof(VSnackbar).GetProperty(nameof(VSnackbar.Timeout), BindingFlags.Public | BindingFlags.Instance)!.PropertyType.UnwrapNullable());
        Assert.AreEqual(typeof(VuetifyRoundedValue), typeof(VSnackbar).GetProperty(nameof(VSnackbar.Rounded), BindingFlags.Public | BindingFlags.Instance)!.PropertyType.UnwrapNullable());
        Assert.AreEqual(typeof(VuetifySelectItems), typeof(VCombobox).GetProperty(nameof(VCombobox.Items), BindingFlags.Public | BindingFlags.Instance)!.PropertyType.UnwrapNullable());
        Assert.AreEqual(typeof(VuetifySelectItemKey), typeof(VCombobox).GetProperty(nameof(VCombobox.ItemTitle), BindingFlags.Public | BindingFlags.Instance)!.PropertyType.UnwrapNullable());
        Assert.AreEqual(typeof(VuetifySelectItemKey), typeof(VCombobox).GetProperty(nameof(VCombobox.ItemValue), BindingFlags.Public | BindingFlags.Instance)!.PropertyType.UnwrapNullable());
        Assert.AreEqual(typeof(VuetifySelectItemPropsSelector), typeof(VCombobox).GetProperty(nameof(VCombobox.ItemProps), BindingFlags.Public | BindingFlags.Instance)!.PropertyType.UnwrapNullable());
        Assert.AreEqual(typeof(VuetifyAutoSelectFirstValue), typeof(VCombobox).GetProperty(nameof(VCombobox.AutoSelectFirst), BindingFlags.Public | BindingFlags.Instance)!.PropertyType.UnwrapNullable());
        Assert.AreEqual(typeof(VuetifySelectModelValue), typeof(VCombobox).GetProperty(nameof(VCombobox.SelectedValue), BindingFlags.Public | BindingFlags.Instance)!.PropertyType.UnwrapNullable());
        Assert.AreEqual(typeof(EventCallback<VuetifySelectModelValue?>), typeof(VCombobox).GetProperty(nameof(VCombobox.SelectedValueChanged), BindingFlags.Public | BindingFlags.Instance)!.PropertyType);
        Assert.AreEqual(typeof(bool), typeof(VFileInput).GetProperty(nameof(VFileInput.Counter), BindingFlags.Public | BindingFlags.Instance)!.PropertyType.UnwrapNullable());
        Assert.AreEqual(typeof(VuetifyFileShowSizeValue), typeof(VFileInput).GetProperty(nameof(VFileInput.ShowSize), BindingFlags.Public | BindingFlags.Instance)!.PropertyType.UnwrapNullable());
        Assert.AreEqual(typeof(VuetifyFileModelValue), typeof(VFileInput).GetProperty(nameof(VFileInput.ModelValue), BindingFlags.Public | BindingFlags.Instance)!.PropertyType.UnwrapNullable());
        Assert.IsNull(typeof(VNumberInput).GetProperty("ControlVariantHidden", BindingFlags.Public | BindingFlags.Instance));
        Assert.AreEqual(typeof(VuetifyNumberInputControlVariant), typeof(VNumberInput).GetProperty(nameof(VNumberInput.ControlVariant), BindingFlags.Public | BindingFlags.Instance)!.PropertyType.UnwrapNullable());
        Assert.AreEqual(typeof(Number), typeof(VNumberInput).GetProperty(nameof(VNumberInput.Min), BindingFlags.Public | BindingFlags.Instance)!.PropertyType.UnwrapNullable());
        Assert.AreEqual(typeof(Number), typeof(VNumberInput).GetProperty(nameof(VNumberInput.ModelValue), BindingFlags.Public | BindingFlags.Instance)!.PropertyType.UnwrapNullable());
        Assert.AreEqual(typeof(string), typeof(VOtpInput).GetProperty(nameof(VOtpInput.Divider), BindingFlags.Public | BindingFlags.Instance)!.PropertyType.UnwrapNullable());
        Assert.AreEqual(typeof(VuetifyInputType), typeof(VOtpInput).GetProperty(nameof(VOtpInput.Type), BindingFlags.Public | BindingFlags.Instance)!.PropertyType.UnwrapNullable());
        Assert.AreEqual(typeof(string), typeof(VRadio).GetProperty(nameof(VRadio.FalseIcon), BindingFlags.Public | BindingFlags.Instance)!.PropertyType.UnwrapNullable());
        Assert.AreEqual(typeof(string), typeof(VRadio).GetProperty(nameof(VRadio.TrueIcon), BindingFlags.Public | BindingFlags.Instance)!.PropertyType.UnwrapNullable());
        Assert.AreEqual(typeof(VuetifyRangeSliderModelValue), typeof(VRangeSlider).GetProperty(nameof(VRangeSlider.ModelValue), BindingFlags.Public | BindingFlags.Instance)!.PropertyType.UnwrapNullable());
        Assert.AreEqual(typeof(Number), typeof(VRangeSlider).GetProperty(nameof(VRangeSlider.Step), BindingFlags.Public | BindingFlags.Instance)!.PropertyType.UnwrapNullable());
        Assert.AreEqual(typeof(VuetifyBooleanAlwaysValue), typeof(VRangeSlider).GetProperty(nameof(VRangeSlider.ThumbLabel), BindingFlags.Public | BindingFlags.Instance)!.PropertyType.UnwrapNullable());
        Assert.AreEqual(typeof(VuetifySliderDirection), typeof(VRangeSlider).GetProperty(nameof(VRangeSlider.Direction), BindingFlags.Public | BindingFlags.Instance)!.PropertyType.UnwrapNullable());
        Assert.AreEqual(typeof(Number), typeof(VSlider).GetProperty(nameof(VSlider.ModelValue), BindingFlags.Public | BindingFlags.Instance)!.PropertyType.UnwrapNullable());
        Assert.AreEqual(typeof(Number), typeof(VSlider).GetProperty(nameof(VSlider.Step), BindingFlags.Public | BindingFlags.Instance)!.PropertyType.UnwrapNullable());
        Assert.AreEqual(typeof(VuetifyBooleanAlwaysValue), typeof(VSlider).GetProperty(nameof(VSlider.ShowTicks), BindingFlags.Public | BindingFlags.Instance)!.PropertyType.UnwrapNullable());
        Assert.AreEqual(typeof(VuetifySliderDirection), typeof(VSlider).GetProperty(nameof(VSlider.Direction), BindingFlags.Public | BindingFlags.Instance)!.PropertyType.UnwrapNullable());
        Assert.AreEqual(typeof(VuetifyBreadcrumbItems), typeof(VBreadcrumbs).GetProperty(nameof(VBreadcrumbs.Items), BindingFlags.Public | BindingFlags.Instance)!.PropertyType.UnwrapNullable());
        Assert.AreEqual(typeof(VuetifyDataTableHeaders), typeof(VDataTable).GetProperty(nameof(VDataTable.Headers), BindingFlags.Public | BindingFlags.Instance)!.PropertyType.UnwrapNullable());
        Assert.AreEqual(typeof(VuetifyDataTableItems), typeof(VDataTable).GetProperty(nameof(VDataTable.Items), BindingFlags.Public | BindingFlags.Instance)!.PropertyType.UnwrapNullable());
        AssertNoPublicMemberUsesObject(
            typeof(VuetifyAutoSelectFirstValue),
            typeof(VuetifyDisplayBreakpoint),
            typeof(VuetifyFileShowSizeValue),
            typeof(VuetifyBooleanAlwaysValue),
            typeof(VuetifyBooleanStringValue),
            typeof(VuetifyCounterValue),
            typeof(VuetifyTextValue),
            typeof(VuetifyRoundedValue),
            typeof(VuetifyProgressCircularIndeterminateValue),
            typeof(VuetifyFileModelValue),
            typeof(VuetifyRangeSliderModelValue),
            typeof(VuetifySelectItems),
            typeof(VuetifySelectModelValues),
            typeof(VuetifySelectModelValue),
            typeof(VuetifySelectItemValue),
            typeof(VuetifySelectItem),
            typeof(VuetifySelectItemKey),
            typeof(VuetifySelectItemPropsSelector),
            typeof(VuetifyItemProps),
            typeof(VuetifyBreadcrumbItems),
            typeof(VuetifyBreadcrumbItemValue),
            typeof(VuetifyBreadcrumbItem),
            typeof(VuetifyDataTableHeaders),
            typeof(VuetifyDataTableHeader),
            typeof(VuetifyDataTableItems),
            typeof(VuetifyDataTableItem));
    }

    [TestMethod]
    public void TDesign_ValueAndUnionTypes_ExposeStronglyTypedContracts()
    {
        CollectionAssert.AreEqual(
            new[] { "button", "reset", "submit" },
            GetStringEnumRuntimeValues(typeof(TDesignButtonType)));
        CollectionAssert.AreEqual(
            new[] { "base", "dashed", "outline", "text" },
            GetStringEnumRuntimeValues(typeof(TDesignButtonVariant)));
        CollectionAssert.AreEqual(
            new[] { "dark", "light" },
            GetStringEnumRuntimeValues(typeof(TDesignMenuTheme)));
        CollectionAssert.AreEqual(
            new[] { "horizontal", "vertical" },
            GetStringEnumRuntimeValues(typeof(TDesignDividerLayout)));
        CollectionAssert.AreEqual(
            new[] { "light" },
            GetStringEnumRuntimeValues(typeof(TDesignBreadcrumbTheme)));
        CollectionAssert.AreEqual(
            new[] { "color", "underline" },
            GetStringEnumRuntimeValues(typeof(TDesignLinkHover)));
        CollectionAssert.AreEqual(
            new[] { "danger", "default", "primary", "success", "warning" },
            GetStringEnumRuntimeValues(typeof(TDesignLinkTheme)));
        CollectionAssert.AreEqual(
            new[] { "bottom", "left", "right", "top" },
            GetStringEnumRuntimeValues(typeof(TDesignTabsPlacement)));
        CollectionAssert.AreEqual(
            new[] { "auto", "center", "end", "start" },
            GetStringEnumRuntimeValues(typeof(TDesignTabsScrollPosition)));
        CollectionAssert.AreEqual(
            new[] { "large", "medium" },
            GetStringEnumRuntimeValues(typeof(TDesignTabsSize)));
        CollectionAssert.AreEqual(
            new[] { "card", "normal" },
            GetStringEnumRuntimeValues(typeof(TDesignTabsTheme)));
        CollectionAssert.AreEqual(
            new[] { "circle", "round" },
            GetStringEnumRuntimeValues(typeof(TDesignAvatarShape)));
        CollectionAssert.AreEqual(
            new[] { "left-up", "right-up" },
            GetStringEnumRuntimeValues(typeof(TDesignAvatarGroupCascading)));
        CollectionAssert.AreEqual(
            new[] { "circle", "round" },
            GetStringEnumRuntimeValues(typeof(TDesignBadgeShape)));
        CollectionAssert.AreEqual(
            new[] { "medium", "small" },
            GetStringEnumRuntimeValues(typeof(TDesignBadgeSize)));

        Assert.AreEqual(typeof(VueStyleValue), typeof(TDesignComponentBase).GetProperty(nameof(TDesignComponentBase.CssStyle), BindingFlags.Public | BindingFlags.Instance)!.PropertyType.UnwrapNullable());
        Assert.AreEqual(typeof(TDesignDimensionValue), typeof(TDesignMenuWidthValue).GetProperty(nameof(TDesignMenuWidthValue.AsValue), BindingFlags.Public | BindingFlags.Instance)!.PropertyType.UnwrapNullable());
        Assert.AreEqual(typeof(TDesignDimensionValues), typeof(TDesignMenuWidthValue).GetProperty(nameof(TDesignMenuWidthValue.AsValues), BindingFlags.Public | BindingFlags.Instance)!.PropertyType.UnwrapNullable());
        Assert.AreEqual(typeof(Number), typeof(TDesignMenuValue).GetProperty(nameof(TDesignMenuValue.AsNumber), BindingFlags.Public | BindingFlags.Instance)!.PropertyType.UnwrapNullable());
        Assert.AreEqual(typeof(string), typeof(TDesignMenuValue).GetProperty(nameof(TDesignMenuValue.AsString), BindingFlags.Public | BindingFlags.Instance)!.PropertyType.UnwrapNullable());
        Assert.AreEqual(typeof(string), typeof(TDesignMenuRouteTarget).GetProperty(nameof(TDesignMenuRouteTarget.AsString), BindingFlags.Public | BindingFlags.Instance)!.PropertyType.UnwrapNullable());
        Assert.AreEqual(typeof(TDesignMenuRoute), typeof(TDesignMenuRouteTarget).GetProperty(nameof(TDesignMenuRouteTarget.AsRoute), BindingFlags.Public | BindingFlags.Instance)!.PropertyType.UnwrapNullable());
        Assert.AreEqual(typeof(MouseEvent), typeof(TDesignMenuItemClickContext).GetProperty(nameof(TDesignMenuItemClickContext.Event), BindingFlags.Public | BindingFlags.Instance)!.PropertyType);
        Assert.AreEqual(typeof(TDesignMenuValue), typeof(TDesignMenuItemClickContext).GetProperty(nameof(TDesignMenuItemClickContext.Value), BindingFlags.Public | BindingFlags.Instance)!.PropertyType);
        Assert.AreEqual(typeof(bool), typeof(TDesignLinkDownloadValue).GetProperty(nameof(TDesignLinkDownloadValue.AsBool), BindingFlags.Public | BindingFlags.Instance)!.PropertyType.UnwrapNullable());
        Assert.AreEqual(typeof(string), typeof(TDesignLinkDownloadValue).GetProperty(nameof(TDesignLinkDownloadValue.AsString), BindingFlags.Public | BindingFlags.Instance)!.PropertyType.UnwrapNullable());
        Assert.AreEqual(typeof(double), typeof(TDesignTabValue).GetProperty(nameof(TDesignTabValue.AsNumber), BindingFlags.Public | BindingFlags.Instance)!.PropertyType.UnwrapNullable());
        Assert.AreEqual(typeof(string), typeof(TDesignTabValue).GetProperty(nameof(TDesignTabValue.AsString), BindingFlags.Public | BindingFlags.Instance)!.PropertyType.UnwrapNullable());
        Assert.AreEqual(typeof(double), typeof(TDesignBadgeCountValue).GetProperty(nameof(TDesignBadgeCountValue.AsNumber), BindingFlags.Public | BindingFlags.Instance)!.PropertyType.UnwrapNullable());
        Assert.AreEqual(typeof(string), typeof(TDesignBadgeCountValue).GetProperty(nameof(TDesignBadgeCountValue.AsString), BindingFlags.Public | BindingFlags.Instance)!.PropertyType.UnwrapNullable());
        Assert.AreEqual(typeof(double), typeof(TDesignBadgeOffsetValue).GetProperty(nameof(TDesignBadgeOffsetValue.AsNumber), BindingFlags.Public | BindingFlags.Instance)!.PropertyType.UnwrapNullable());
        Assert.AreEqual(typeof(string), typeof(TDesignBadgeOffsetValue).GetProperty(nameof(TDesignBadgeOffsetValue.AsString), BindingFlags.Public | BindingFlags.Instance)!.PropertyType.UnwrapNullable());
        Assert.AreEqual(typeof(TDesignBadgeOffsetValue[]), typeof(TDesignBadgeOffset).GetProperty(nameof(TDesignBadgeOffset.AsValues), BindingFlags.Public | BindingFlags.Instance)!.PropertyType.UnwrapNullable());
        Assert.AreEqual(typeof(MouseEvent), typeof(TDesignTabAddContext).GetProperty(nameof(TDesignTabAddContext.Event), BindingFlags.Public | BindingFlags.Instance)!.PropertyType);
        Assert.AreEqual(typeof(TDesignTabValue), typeof(TDesignTabRemoveContext).GetProperty(nameof(TDesignTabRemoveContext.Value), BindingFlags.Public | BindingFlags.Instance)!.PropertyType);
        Assert.AreEqual(typeof(int), typeof(TDesignTabRemoveContext).GetProperty(nameof(TDesignTabRemoveContext.Index), BindingFlags.Public | BindingFlags.Instance)!.PropertyType);
        Assert.AreEqual(typeof(MouseEvent), typeof(TDesignTabRemoveContext).GetProperty(nameof(TDesignTabRemoveContext.Event), BindingFlags.Public | BindingFlags.Instance)!.PropertyType);
        Assert.AreEqual(typeof(TDesignTabValue), typeof(TDesignTabPanelRemoveContext).GetProperty(nameof(TDesignTabPanelRemoveContext.Value), BindingFlags.Public | BindingFlags.Instance)!.PropertyType);
        Assert.AreEqual(typeof(MouseEvent), typeof(TDesignTabPanelRemoveContext).GetProperty(nameof(TDesignTabPanelRemoveContext.Event), BindingFlags.Public | BindingFlags.Instance)!.PropertyType);
        Assert.AreEqual(typeof(int), typeof(TDesignTabsDragSortContext).GetProperty(nameof(TDesignTabsDragSortContext.CurrentIndex), BindingFlags.Public | BindingFlags.Instance)!.PropertyType);
        Assert.AreEqual(typeof(TDesignTabValue), typeof(TDesignTabsDragSortContext).GetProperty(nameof(TDesignTabsDragSortContext.Current), BindingFlags.Public | BindingFlags.Instance)!.PropertyType);
        Assert.AreEqual(typeof(int), typeof(TDesignTabsDragSortContext).GetProperty(nameof(TDesignTabsDragSortContext.TargetIndex), BindingFlags.Public | BindingFlags.Instance)!.PropertyType);
        Assert.AreEqual(typeof(TDesignTabValue), typeof(TDesignTabsDragSortContext).GetProperty(nameof(TDesignTabsDragSortContext.Target), BindingFlags.Public | BindingFlags.Instance)!.PropertyType);
        Assert.AreEqual(typeof(Event), typeof(TDesignAvatarErrorContext).GetProperty(nameof(TDesignAvatarErrorContext.Event), BindingFlags.Public | BindingFlags.Instance)!.PropertyType);

        AssertNoPublicMemberUsesObject(
            typeof(TDesignDimensionValue),
            typeof(TDesignDimensionValues),
            typeof(TDesignMenuWidthValue),
            typeof(TDesignSpaceSizeValue),
            typeof(TDesignSpaceSizeValues),
            typeof(TDesignSpaceSize),
            typeof(TDesignMenuValue),
            typeof(TDesignMenuQueryValue),
            typeof(TDesignMenuRouteTarget),
            typeof(TDesignLinkDownloadValue),
            typeof(TDesignTabValue),
            typeof(TDesignBadgeCountValue),
            typeof(TDesignBadgeOffsetValue),
            typeof(TDesignBadgeOffset));

        AssertNotObject(typeof(TDesignGlobalConfig), nameof(TDesignGlobalConfig));
        AssertNotObject(typeof(TDesignComponentRegistry), nameof(TDesignComponentRegistry));
        AssertNotObject(typeof(TDesignStyles), nameof(TDesignStyles));
        AssertNotObject(typeof(TDesignMenuQueryData), nameof(TDesignMenuQueryData));
        AssertNotObject(typeof(TDesignMenuRoute), nameof(TDesignMenuRoute));
        AssertNotObject(typeof(TDesignMenuItemClickContext), nameof(TDesignMenuItemClickContext));
        AssertNotObject(typeof(TDesignTabAddContext), nameof(TDesignTabAddContext));
        AssertNotObject(typeof(TDesignTabRemoveContext), nameof(TDesignTabRemoveContext));
        AssertNotObject(typeof(TDesignTabPanelRemoveContext), nameof(TDesignTabPanelRemoveContext));
        AssertNotObject(typeof(TDesignTabsDragSortContext), nameof(TDesignTabsDragSortContext));
        AssertNotObject(typeof(TDesignAvatarErrorContext), nameof(TDesignAvatarErrorContext));
    }

    private static string[] GetStringEnumRuntimeValues(Type enumType)
        => enumType
            .GetFields(BindingFlags.Public | BindingFlags.Static)
            .Select(static field => field.GetCustomAttribute<System.ComponentModel.DescriptionAttribute>()?.Description)
            .Select(static description => description?.TrimStart('@', '#'))
            .OrderBy(static value => value, StringComparer.Ordinal)
            .ToArray()!;

    private static void AssertNotObject(Type type, string message)
    {
        Assert.AreNotEqual(typeof(object), type.UnwrapNullable(), message);

        if (!type.IsGenericType)
            return;

        foreach (var argument in type.GetGenericArguments())
            AssertNotObject(argument, message);
    }

    private static void AssertNoPublicMemberUsesObject(params Type[] types)
    {
        foreach (var type in types)
        {
            AssertNotObject(type, type.FullName ?? type.Name);

            foreach (var property in type.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly))
            {
                if (IsUnionValueProperty(property))
                    continue;

                AssertNotObject(property.PropertyType, $"{type.Name}.{property.Name}");
            }

            foreach (var method in type.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly)
                         .Where(static method => !method.IsSpecialName && !IsRecordRuntimeMethod(method)))
            {
                AssertNotObject(method.ReturnType, $"{type.Name}.{method.Name} return");
                foreach (var parameter in method.GetParameters())
                    AssertNotObject(parameter.ParameterType, $"{type.Name}.{method.Name}({parameter.Name})");
            }
        }
    }

    private static void AssertNet11UnionContract(Type unionType, params Type[] constructorBranchTypes)
    {
        Assert.IsNotNull(unionType.GetCustomAttribute<System.Runtime.CompilerServices.UnionAttribute>(), unionType.FullName);
        Assert.IsTrue(typeof(System.Runtime.CompilerServices.IUnion).IsAssignableFrom(unionType), unionType.FullName);

        var value = unionType.GetProperty(nameof(System.Runtime.CompilerServices.IUnion.Value), BindingFlags.Public | BindingFlags.Instance);
        Assert.IsNotNull(value, unionType.FullName);
        Assert.AreEqual(typeof(object), value!.PropertyType);

        CollectionAssert.AreEquivalent(
            constructorBranchTypes,
            unionType
                .GetConstructors(BindingFlags.Public | BindingFlags.Instance)
                .Select(static constructor => constructor.GetParameters().SingleOrDefault()?.ParameterType)
                .Where(static type => type is not null)
                .ToArray(),
            unionType.FullName);

        AssertNoAssignableBranchOverlap(unionType, constructorBranchTypes);
    }

    private static void AssertNoAssignableBranchOverlap(Type unionType, Type[] constructorBranchTypes)
    {
        foreach (var left in constructorBranchTypes)
        foreach (var right in constructorBranchTypes)
        {
            if (left == right)
                continue;

            Assert.IsFalse(
                left.IsAssignableFrom(right),
                $"{unionType.FullName} cannot use native union because branch {right.FullName} is assignable to {left.FullName}; keep a tagged [Union] + IUnion wrapper to preserve exact AsX projections.");
        }
    }

    private static bool IsRecordRuntimeMethod(MethodInfo method)
        => method.Name is nameof(object.Equals) or nameof(object.GetHashCode) or nameof(ToString) &&
           method.DeclaringType?.IsAssignableTo(typeof(VueProps)) == true;

    private static bool IsUnionValueProperty(PropertyInfo property)
        => property.Name == nameof(System.Runtime.CompilerServices.IUnion.Value) &&
           typeof(System.Runtime.CompilerServices.IUnion).IsAssignableFrom(property.DeclaringType);

    private static Type GetPropertyType(PropertyInfo? property)
    {
        Assert.IsNotNull(property);
        return property!.PropertyType.UnwrapNullable();
    }

    private static Type[] GetUnionConstructorBranchTypes(Type unionType)
        => unionType
            .GetConstructors(BindingFlags.Public | BindingFlags.Instance)
            .Select(static constructor => constructor.GetParameters().SingleOrDefault()?.ParameterType)
            .Where(static type => type is not null)
            .ToArray()!;

    private static void AssertEcmaScriptImport(Type type, string expectedImport)
    {
        var runtime = type.GetCustomAttribute<ECMAScriptAttribute>();
        var module = type.GetCustomAttribute<ECMAScriptModuleAttribute>();

        Assert.IsNotNull(runtime, type.FullName);
        Assert.IsNull(module, type.FullName);
        Assert.AreEqual(expectedImport, runtime!.Import, type.FullName);
    }

    private static void AssertEcmaScriptSupport(Type type)
    {
        var runtime = type.GetCustomAttribute<ECMAScriptAttribute>();
        var module = type.GetCustomAttribute<ECMAScriptModuleAttribute>();

        Assert.IsNotNull(runtime, type.FullName);
        Assert.IsNull(module, type.FullName);
        Assert.IsNull(runtime!.Import, type.FullName);
    }

    private static bool HasAttribute(MemberInfo member, string attributeTypeName)
        => member.GetCustomAttributesData().Any(attribute => attribute.AttributeType.Name == attributeTypeName);

    private static void AssertNoHostInferenceAttribute(Type type, string propertyName)
    {
        var property = type.GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance);
        Assert.IsNotNull(property, $"{type.FullName}.{propertyName}");

        var attributeTypeNames = property!.GetCustomAttributesData()
            .Select(static attribute => attribute.AttributeType.FullName)
            .ToArray();
        CollectionAssert.DoesNotContain(attributeTypeNames, typeof(PropsAttribute).FullName, $"{type.FullName}.{propertyName}");
        CollectionAssert.DoesNotContain(attributeTypeNames, typeof(EmitsAttribute).FullName, $"{type.FullName}.{propertyName}");
    }
}

internal static class TypeTestExtensions
{
    public static Type UnwrapNullable(this Type type)
        => Nullable.GetUnderlyingType(type) ?? type;
}

public sealed record TestVueProps : VueProps;

public sealed record TestVueSlots : VueSlots;

public sealed record TestVuePluginOptions : VuePluginOptions;

#pragma warning restore CA1416
