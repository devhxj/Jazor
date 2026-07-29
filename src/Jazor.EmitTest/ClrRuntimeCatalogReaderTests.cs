using System.Text.RegularExpressions;
using Jazor.Emit;

namespace Jazor.EmitTest;

[TestClass]
public sealed class ClrRuntimeCatalogReaderTests
{
    [TestMethod]
    public void CatalogReader_TryRead_UsesECMAScriptDedicatedCatalogTypeName()
    {
        var catalogType = typeof(ECMAScript.Number).Assembly.GetType("ECMAScript.Catalog", throwOnError: false, ignoreCase: false);

        Assert.IsNotNull(catalogType);
        Assert.IsNull(typeof(ECMAScript.Number).Assembly.GetType("Jazor.Generated.ModuleCatalog", throwOnError: false, ignoreCase: false));
    }

    [TestMethod]
    public void CatalogReader_TryRead_ReadsClrRuntimeModules_FromEcmascriptAssembly()
    {
        var assembly = typeof(ECMAScript.Number).Assembly;

        var modules = CatalogReader.TryRead(assembly);

        Assert.IsNotNull(modules);
        Assert.IsTrue(modules.Count >= 30, $"Expected at least 30 CLR runtime modules, but found {modules.Count}.");

        AssertContainsModule(modules, "System/RuntimeModule.js");
        AssertContainsModule(modules, "System/StringModule.js");
        AssertContainsModule(modules, "System/DecimalModule.js");
        AssertContainsModule(modules, "System/Globalization/CultureInfoModule.js");
    }

    [TestMethod]
    public void ModuleCollector_Collect_ReadsClrRuntimeModules_FromEcmascriptAssemblyCatalog()
    {
        var assemblyPath = typeof(ECMAScript.Number).Assembly.Location;
        var loadContext = new EmitLoadContext(assemblyPath);
        var collector = new ModuleCollector(loadContext);
        collector.AddAssembly(assemblyPath);

        var result = collector.Collect(failOnPathConflict: true);

        Assert.IsTrue(result.IsSuccess, result.Error ?? string.Empty);
        Assert.AreEqual(1, result.AssemblyCount);
        Assert.AreEqual(1, result.CatalogCount);

        AssertContainsModule(result.Modules, "System/RuntimeModule.js");
        AssertContainsModule(result.Modules, "System/StringModule.js");
        AssertContainsModule(result.Modules, "System/DecimalModule.js");
        AssertContainsModule(result.Modules, "System/Globalization/CultureInfoModule.js");
    }

    [TestMethod]
    public void CatalogReader_TryRead_ExportsClrImportMembers_WithoutWholeModuleNamespaceObjects()
    {
        var assembly = typeof(ECMAScript.Number).Assembly;

        var modules = CatalogReader.TryRead(assembly);

        Assert.IsNotNull(modules);

        var stringModule = modules.Single(module => string.Equals(module.RelativePath, "System/StringModule.js", StringComparison.OrdinalIgnoreCase));
        StringAssert.Contains(stringModule.Content, "export function _5ad63706a889c294");
        Assert.IsFalse(stringModule.Content.Contains("export const StringModule = {", StringComparison.Ordinal), stringModule.Content);

        var runtimeModule = modules.Single(module => string.Equals(module.RelativePath, "System/RuntimeModule.js", StringComparison.OrdinalIgnoreCase));
        Assert.IsFalse(runtimeModule.Content.Contains("from \"System/RuntimeModule.js\"", StringComparison.Ordinal), runtimeModule.Content);
        Assert.IsFalse(runtimeModule.Content.Contains("import {", StringComparison.Ordinal), runtimeModule.Content);
        Assert.IsFalse(runtimeModule.Content.Contains("export const RuntimeModule = {", StringComparison.Ordinal), runtimeModule.Content);
        var queueStart = runtimeModule.Content.IndexOf("export class JQueue {", StringComparison.Ordinal);
        var queueEnd = runtimeModule.Content.IndexOf("export class JStack {", queueStart, StringComparison.Ordinal);
        Assert.IsGreaterThanOrEqualTo(0, queueStart, runtimeModule.Content);
        Assert.IsGreaterThan(queueStart, queueEnd, runtimeModule.Content);

        var queueClass = runtimeModule.Content.Substring(queueStart, queueEnd - queueStart);
        var itemsGetter = Regex.Match(
            queueClass,
            @"get items\(\)\s*\{\s*return this\.(?<field>#[A-Za-z0-9_$]+);\s*\}",
            RegexOptions.CultureInvariant);
        Assert.IsTrue(itemsGetter.Success, queueClass);
        StringAssert.Contains(
            queueClass,
            "this." + itemsGetter.Groups["field"].Value + " = materializeArray(collection, ");
        StringAssert.Contains(runtimeModule.Content, "return new JQueue(\"$ctor_");

        var byteModule = modules.Single(module => string.Equals(module.RelativePath, "System/ByteModule.js", StringComparison.OrdinalIgnoreCase));
        StringAssert.Contains(byteModule.Content, "export function _8719e4b3055c5188");
        Assert.IsFalse(byteModule.Content.Contains("export const ByteModule = {", StringComparison.Ordinal), byteModule.Content);

        var comparerModule = modules.Single(module => string.Equals(module.RelativePath, "System/Collections/Generic/ComparerT1Module.js", StringComparison.OrdinalIgnoreCase));
        Assert.IsFalse(comparerModule.Content.Contains("export const ComparerT1Module = {", StringComparison.Ordinal), comparerModule.Content);
        StringAssert.Contains(comparerModule.Content, "export function ensureComparerInstance");
        StringAssert.Contains(comparerModule.Content, "export function compareCore");
    }

    [TestMethod]
    public void CatalogReader_TryRead_StringIndexerGetter_UsesCharAt_WithoutSelfRecursiveImport()
    {
        var assembly = typeof(ECMAScript.Number).Assembly;

        var modules = CatalogReader.TryRead(assembly);

        Assert.IsNotNull(modules);

        var stringModule = modules.Single(module => string.Equals(module.RelativePath, "System/StringModule.js", StringComparison.OrdinalIgnoreCase));
        StringAssert.Contains(stringModule.Content, "return instance.charAt(index);");
        Assert.IsFalse(stringModule.Content.Contains("return String.fromCharCode(i$8578349aab59a79b(instance, index));", StringComparison.Ordinal));
    }

    [TestMethod]
    public void CatalogReader_TryRead_ClrBigIntStatics_InlineToNativeLiterals()
    {
        var assembly = typeof(ECMAScript.Number).Assembly;

        var modules = CatalogReader.TryRead(assembly);

        Assert.IsNotNull(modules);

        var allContent = string.Join("\n", modules.Select(static module => module.Content));
        Assert.IsFalse(allContent.Contains("BigInt.zero", StringComparison.Ordinal), "CLR runtime catalog still contains invalid BigInt.zero access.");
        Assert.IsFalse(allContent.Contains("BigInt.one", StringComparison.Ordinal), "CLR runtime catalog still contains invalid BigInt.one access.");
        Assert.IsFalse(allContent.Contains("BigInt.minusOne", StringComparison.Ordinal), "CLR runtime catalog still contains invalid BigInt.minusOne access.");

        var dateTimeModule = modules.Single(module => string.Equals(module.RelativePath, "System/DateTimeModule.js", StringComparison.OrdinalIgnoreCase));
        StringAssert.Contains(dateTimeModule.Content, "function get_ZeroTicks() {\n  return 0n;\n}");
    }

    [TestMethod]
    public void CatalogReader_TryRead_ClrIntlHostsAndPropertyDescriptors_UseStandardJsForms()
    {
        var assembly = typeof(ECMAScript.Number).Assembly;

        var modules = CatalogReader.TryRead(assembly);

        Assert.IsNotNull(modules);

        var allContent = string.Join("\n", modules.Select(static module => module.Content));
        Assert.IsFalse(allContent.Contains("instanceof NumberFormat", StringComparison.Ordinal), "CLR runtime catalog still contains a bare NumberFormat instanceof check.");
        Assert.IsFalse(allContent.Contains("new NumberFormat", StringComparison.Ordinal), "CLR runtime catalog still contains a bare NumberFormat constructor.");
        Assert.IsFalse(allContent.Contains("new DateTimeFormat", StringComparison.Ordinal), "CLR runtime catalog still contains a bare DateTimeFormat constructor.");
        Assert.IsFalse(allContent.Contains("new Locale", StringComparison.Ordinal), "CLR runtime catalog still contains a bare Locale constructor.");
        Assert.IsFalse(allContent.Contains("new DisplayNames", StringComparison.Ordinal), "CLR runtime catalog still contains a bare DisplayNames constructor.");
        Assert.IsFalse(allContent.Contains("new PropertyDescriptor", StringComparison.Ordinal), "CLR runtime catalog still contains a non-standard PropertyDescriptor constructor.");

        var dateTimeModule = modules.Single(module => string.Equals(module.RelativePath, "System/DateTimeModule.js", StringComparison.OrdinalIgnoreCase));
        StringAssert.Contains(dateTimeModule.Content, "provider instanceof Intl.NumberFormat");
        StringAssert.Contains(dateTimeModule.Content, "new Intl.DateTimeFormat");

        var cultureInfoModule = modules.Single(module => string.Equals(module.RelativePath, "System/Globalization/CultureInfoModule.js", StringComparison.OrdinalIgnoreCase));
        StringAssert.Contains(cultureInfoModule.Content, "new Intl.Locale(normalized)");
        StringAssert.Contains(cultureInfoModule.Content, "new Intl.DisplayNames(");
        StringAssert.Contains(cultureInfoModule.Content, "type: \"language\"");
        StringAssert.Contains(cultureInfoModule.Content, "fallback: \"code\"");
        StringAssert.Contains(cultureInfoModule.Content, "languageDisplay: \"dialect\"");

        var runtimeModule = modules.Single(module => string.Equals(module.RelativePath, "System/RuntimeModule.js", StringComparison.OrdinalIgnoreCase));
        StringAssert.Contains(runtimeModule.Content, "Object.defineProperty(this, Symbol.toPrimitive, {");

        StringAssert.Contains(dateTimeModule.Content, "month: abbreviated ? \"short\" : \"long\"");
        StringAssert.Contains(dateTimeModule.Content, "weekday: abbreviated ? \"short\" : \"long\"");
        StringAssert.Contains(dateTimeModule.Content, "year: \"numeric\"");
        Assert.IsFalse(dateTimeModule.Content.Contains("month: abbreviated ? 1 : 0", StringComparison.Ordinal), dateTimeModule.Content);
    }

    [TestMethod]
    public void CatalogReader_TryRead_ClrRuntimeHelpers_UseCorrectArraySortRotateAndRegexForms()
    {
        var assembly = typeof(ECMAScript.Number).Assembly;

        var modules = CatalogReader.TryRead(assembly);

        Assert.IsNotNull(modules);

        var arrayModule = modules.Single(module => string.Equals(module.RelativePath, "System/ArrayModule.js", StringComparison.OrdinalIgnoreCase));
        StringAssert.Contains(arrayModule.Content, "let newArray = new Array(newSize);");
        StringAssert.Contains(arrayModule.Content, "from \"System/Collections/Generic/ComparerT1Module.js\";");
        StringAssert.Contains(arrayModule.Content, "compareCore");
        StringAssert.Contains(arrayModule.Content, "compareObjectsCore");
        Assert.IsFalse(arrayModule.Content.Contains(".toString()", StringComparison.Ordinal), arrayModule.Content);
        Assert.IsFalse(arrayModule.Content.Contains("let newArray = [newSize];", StringComparison.Ordinal), arrayModule.Content);
        Assert.IsFalse(arrayModule.Content.Contains("array.sort();", StringComparison.Ordinal), arrayModule.Content);
        Assert.IsFalse(arrayModule.Content.Contains("subArray.sort();", StringComparison.Ordinal), arrayModule.Content);
        Assert.IsFalse(arrayModule.Content.Contains("keys.sort();", StringComparison.Ordinal), arrayModule.Content);

        var listModule = modules.Single(module => string.Equals(module.RelativePath, "System/Collections/Generic/ListT1Module.js", StringComparison.OrdinalIgnoreCase));
        Assert.IsFalse(listModule.Content.Contains("instance.sort();", StringComparison.Ordinal), listModule.Content);
        Assert.IsFalse(listModule.Content.Contains("subArray.sort();", StringComparison.Ordinal), listModule.Content);

        var comparerModule = modules.Single(module => string.Equals(module.RelativePath, "System/Collections/Generic/ComparerT1Module.js", StringComparison.OrdinalIgnoreCase));
        StringAssert.Contains(comparerModule.Content, "return isNaN(rightNumber) ? 0 : 1;");
        StringAssert.Contains(comparerModule.Content, "if (isNaN(rightNumber))\n      return -1;");
        StringAssert.Contains(comparerModule.Content, "throw new Error(\"ArgumentException: At least one object must implement IComparable.\");");
        Assert.IsFalse(comparerModule.Content.Contains("let leftText = x.toString();", StringComparison.Ordinal), comparerModule.Content);
        Assert.IsFalse(comparerModule.Content.Contains("let rightText = y.toString();", StringComparison.Ordinal), comparerModule.Content);

        var doubleModule = modules.Single(module => string.Equals(module.RelativePath, "System/DoubleModule.js", StringComparison.OrdinalIgnoreCase));
        Assert.IsFalse(doubleModule.Content.Contains("export function _aed2927097617729", StringComparison.Ordinal), doubleModule.Content);
        Assert.IsFalse(doubleModule.Content.Contains("export function _24e14b276e0c7e30", StringComparison.Ordinal), doubleModule.Content);
        Assert.IsFalse(doubleModule.Content.Contains("export const DoubleModule = {", StringComparison.Ordinal), doubleModule.Content);

        var int64Module = modules.Single(module => string.Equals(module.RelativePath, "System/Int64Module.js", StringComparison.OrdinalIgnoreCase));
        StringAssert.Contains(int64Module.Content, "function normalizeRotateBits(value)");
        StringAssert.Contains(int64Module.Content, "function normalizeSignedRotateResult(value)");
        Assert.IsFalse(int64Module.Content.Contains("return value << amount | value >> BigInt(64) - amount;", StringComparison.Ordinal), int64Module.Content);
        Assert.IsFalse(int64Module.Content.Contains("return value >> amount | value << BigInt(64) - amount;", StringComparison.Ordinal), int64Module.Content);

        var stringModule = modules.Single(module => string.Equals(module.RelativePath, "System/StringModule.js", StringComparison.OrdinalIgnoreCase));
        StringAssert.Contains(stringModule.Content, "return ch;");
        Assert.IsFalse(stringModule.Content.Contains("return String.fromCharCode(ch);", StringComparison.Ordinal), stringModule.Content);
        Assert.IsFalse(stringModule.Content.Contains("export const StringModule = {", StringComparison.Ordinal), stringModule.Content);

        var decimalModule = modules.Single(module => string.Equals(module.RelativePath, "System/DecimalModule.js", StringComparison.OrdinalIgnoreCase));
        StringAssert.Contains(decimalModule.Content, "function getNumberStylesValue(style) {");
        StringAssert.Contains(decimalModule.Content, "function getMidpointRoundingValue(mode) {");
        Assert.IsFalse(decimalModule.Content.Contains("enumStyle = style", StringComparison.Ordinal), decimalModule.Content);
        Assert.IsFalse(decimalModule.Content.Contains("enumMode = mode", StringComparison.Ordinal), decimalModule.Content);

        var dateOnlyModule = modules.Single(module => string.Equals(module.RelativePath, "System/DateOnlyModule.js", StringComparison.OrdinalIgnoreCase));
        Assert.IsFalse(dateOnlyModule.Content.Contains("enumStyle = style", StringComparison.Ordinal), dateOnlyModule.Content);

        var timeOnlyModule = modules.Single(module => string.Equals(module.RelativePath, "System/TimeOnlyModule.js", StringComparison.OrdinalIgnoreCase));
        Assert.IsFalse(timeOnlyModule.Content.Contains("enumStyle = style", StringComparison.Ordinal), timeOnlyModule.Content);

        var byteModule = modules.Single(module => string.Equals(module.RelativePath, "System/ByteModule.js", StringComparison.OrdinalIgnoreCase));
        StringAssert.Contains(byteModule.Content, "OverflowException: Value was either too large or too small for an unsigned byte.");

        var uint16Module = modules.Single(module => string.Equals(module.RelativePath, "System/UInt16Module.js", StringComparison.OrdinalIgnoreCase));
        StringAssert.Contains(uint16Module.Content, "OverflowException: Value was either too large or too small for a UInt16.");
        Assert.IsFalse(uint16Module.Content.Contains("else if (_5ad63706a889c294(trimmed, 0) === \"-\")", StringComparison.Ordinal), uint16Module.Content);

        var uint32Module = modules.Single(module => string.Equals(module.RelativePath, "System/UInt32Module.js", StringComparison.OrdinalIgnoreCase));
        StringAssert.Contains(uint32Module.Content, "OverflowException: Value was either too large or too small for a UInt32.");
        Assert.IsFalse(uint32Module.Content.Contains("else if (_5ad63706a889c294(trimmed, 0) === \"-\")", StringComparison.Ordinal), uint32Module.Content);

        var timeSpanModule = modules.Single(module => string.Equals(module.RelativePath, "System/TimeSpanModule.js", StringComparison.OrdinalIgnoreCase));
        StringAssert.Contains(timeSpanModule.Content, "from \"System/RuntimeModule.js\";");
        StringAssert.Contains(timeSpanModule.Content, "JTimeSpan");
        StringAssert.Contains(timeSpanModule.Content, "getInt64HashCode");
        Assert.IsFalse(timeSpanModule.Content.Contains("import { RuntimeModule } from \"System/RuntimeModule.js\";", StringComparison.Ordinal), timeSpanModule.Content);
        Assert.IsFalse(timeSpanModule.Content.Contains("RuntimeModule,", StringComparison.Ordinal), timeSpanModule.Content);
    }

    [TestMethod]
    public void CatalogReader_TryRead_ClrCharModule_UsesStringCarrierSemantics()
    {
        var assembly = typeof(ECMAScript.Number).Assembly;

        var modules = CatalogReader.TryRead(assembly);

        Assert.IsNotNull(modules);

        var charModule = modules.Single(module => string.Equals(module.RelativePath, "System/CharModule.js", StringComparison.OrdinalIgnoreCase));
        StringAssert.Contains(charModule.Content, "typeof value !== \"string\"");
        StringAssert.Contains(charModule.Content, "return [false, \"\\0\"];");
        StringAssert.Contains(charModule.Content, "let code = getCodeUnit(c);");
        StringAssert.Contains(charModule.Content, "return isWhiteSpaceCode(getCodeUnit(c));");
        StringAssert.Contains(charModule.Content, "code === 5760");
        StringAssert.Contains(charModule.Content, "code >= 8192 && code <= 8202");
        StringAssert.Contains(charModule.Content, "code === 8232");
        StringAssert.Contains(charModule.Content, "code === 8233");
        StringAssert.Contains(charModule.Content, "code === 8239");
        StringAssert.Contains(charModule.Content, "code === 8287");
        StringAssert.Contains(charModule.Content, "code === 12288");
        StringAssert.Contains(charModule.Content, "return isControlCode(c);");
        StringAssert.Contains(charModule.Content, "return c.charCodeAt(0) >= \"A\".charCodeAt(0) && c.charCodeAt(0) <= \"Z\".charCodeAt(0) || c.charCodeAt(0) >= \"a\".charCodeAt(0) && c.charCodeAt(0) <= \"z\".charCodeAt(0);");
        StringAssert.Contains(charModule.Content, "getCodeUnitFromChar(_5ad63706a889c294(s, index))");
        Assert.IsFalse(charModule.Content.Contains("typeof value !== \"number\"", StringComparison.Ordinal), charModule.Content);
        Assert.IsFalse(charModule.Content.Contains("return [false, 0];", StringComparison.Ordinal), charModule.Content);
        Assert.IsFalse(charModule.Content.Contains("return c < 32 || c === 127;", StringComparison.Ordinal), charModule.Content);
        Assert.IsFalse(charModule.Content.Contains("return c >= \"A\" && c <= \"Z\"", StringComparison.Ordinal), charModule.Content);
    }

    [TestMethod]
    public void CatalogReader_TryRead_ClrBigIntegerModule_FollowsDotNetLogLeadingZeroAndModPowSemantics()
    {
        var assembly = typeof(ECMAScript.Number).Assembly;

        var modules = CatalogReader.TryRead(assembly);

        Assert.IsNotNull(modules);

        var bigIntegerModule = modules.Single(module => string.Equals(module.RelativePath, "System/Numerics/BigIntegerModule.js", StringComparison.OrdinalIgnoreCase));
        StringAssert.Contains(bigIntegerModule.Content, "function computePositiveLog(value, baseValue)");
        StringAssert.Contains(bigIntegerModule.Content, "if (value < 0n || baseValue === 1)");
        StringAssert.Contains(bigIntegerModule.Content, "if (baseValue === Number.POSITIVE_INFINITY)");
        StringAssert.Contains(bigIntegerModule.Content, "return Math.log(0) / Math.log(baseValue);");
        StringAssert.Contains(bigIntegerModule.Content, "return BigInt(32);");
        Assert.IsFalse(bigIntegerModule.Content.Contains("return BigInt(64);", StringComparison.Ordinal), bigIntegerModule.Content);
        StringAssert.Contains(bigIntegerModule.Content, "let modulusMagnitude = modulus < 0n ? -modulus : modulus;");
        StringAssert.Contains(bigIntegerModule.Content, "let negativeResult = value < 0n && (exponent & 1n) === 1n;");
        Assert.IsFalse(bigIntegerModule.Content.Contains("throw new RangeError(\"Logarithm is undefined for non-positive numbers\")", StringComparison.Ordinal), bigIntegerModule.Content);
        Assert.IsFalse(bigIntegerModule.Content.Contains("throw new RangeError(\"Base must be positive and not equal to 1\")", StringComparison.Ordinal), bigIntegerModule.Content);
    }

    [TestMethod]
    public void CatalogReader_TryRead_ClrCarrierHelpers_UseDirectNamedImports_WithoutDedicatedCarrierModules()
    {
        var assembly = typeof(ECMAScript.Number).Assembly;

        var modules = CatalogReader.TryRead(assembly);

        Assert.IsNotNull(modules);

        var dictionaryModule = modules.Single(module => string.Equals(module.RelativePath, "System/Collections/Generic/IDictionaryT2Module.js", StringComparison.OrdinalIgnoreCase));
        StringAssert.Contains(dictionaryModule.Content, "import { isReadOnlyDictionaryCarrier } from \"System/RuntimeModule.js\";");
        Assert.IsFalse(dictionaryModule.Content.Contains("RuntimeModule,", StringComparison.Ordinal), dictionaryModule.Content);

        var readOnlyDictionaryModule = modules.Single(module => string.Equals(module.RelativePath, "System/Collections/ObjectModel/ReadOnlyDictionaryT2Module.js", StringComparison.OrdinalIgnoreCase));
        StringAssert.Contains(readOnlyDictionaryModule.Content, "import { markAsReadOnlyDictionaryCarrier } from \"System/RuntimeModule.js\";");
        Assert.IsFalse(readOnlyDictionaryModule.Content.Contains("RuntimeModule,", StringComparison.Ordinal), readOnlyDictionaryModule.Content);

        var setModule = modules.Single(module => string.Equals(module.RelativePath, "System/Collections/Generic/ISetT1Module.js", StringComparison.OrdinalIgnoreCase));
        StringAssert.Contains(setModule.Content, "import { isReadOnlySetCarrier } from \"System/RuntimeModule.js\";");
        Assert.IsFalse(setModule.Content.Contains("RuntimeModule,", StringComparison.Ordinal), setModule.Content);

        var readOnlySetModule = modules.Single(module => string.Equals(module.RelativePath, "System/Collections/ObjectModel/ReadOnlySetT1Module.js", StringComparison.OrdinalIgnoreCase));
        StringAssert.Contains(readOnlySetModule.Content, "import { markAsReadOnlySetCarrier } from \"System/RuntimeModule.js\";");
        Assert.IsFalse(readOnlySetModule.Content.Contains("RuntimeModule,", StringComparison.Ordinal), readOnlySetModule.Content);

        Assert.IsFalse(modules.Any(module => string.Equals(module.RelativePath, "System/Collections/Generic/DictionaryCarrierRuntime.js", StringComparison.OrdinalIgnoreCase)));
        Assert.IsFalse(modules.Any(module => string.Equals(module.RelativePath, "System/Collections/Generic/SetCarrierRuntime.js", StringComparison.OrdinalIgnoreCase)));

        var runtimeModule = modules.Single(module => string.Equals(module.RelativePath, "System/RuntimeModule.js", StringComparison.OrdinalIgnoreCase));
        StringAssert.Contains(runtimeModule.Content, "export function isReadOnlySetCarrier");
        StringAssert.Contains(runtimeModule.Content, "export function markAsReadOnlySetCarrier");
        StringAssert.Contains(runtimeModule.Content, "export function isReadOnlyDictionaryCarrier");
        StringAssert.Contains(runtimeModule.Content, "export function markAsReadOnlyDictionaryCarrier");
        StringAssert.Contains(runtimeModule.Content, "Object.hasOwn(instance, \"__jazor$readonly\")");
        Assert.IsFalse(runtimeModule.Content.Contains("__jazor$readonly_set", StringComparison.Ordinal), runtimeModule.Content);
        Assert.IsFalse(runtimeModule.Content.Contains("__jazor$readonly_dictionary", StringComparison.Ordinal), runtimeModule.Content);
        Assert.IsFalse(runtimeModule.Content.Contains("Object.getOwnPropertyDescriptor(instance, \"__jazor$readonly\")", StringComparison.Ordinal), runtimeModule.Content);
    }

    [TestMethod]
    public void CatalogReader_TryRead_ClrTimeDoubleHelpers_FollowCurrentDotNetSemantics()
    {
        var assembly = typeof(ECMAScript.Number).Assembly;

        var modules = CatalogReader.TryRead(assembly);

        Assert.IsNotNull(modules);

        var timeSpanModule = modules.Single(module => string.Equals(module.RelativePath, "System/TimeSpanModule.js", StringComparison.OrdinalIgnoreCase));
        StringAssert.Contains(timeSpanModule.Content, "function roundToEven(value)");
        StringAssert.Contains(timeSpanModule.Content, "function createFromTruncatedTicks(value)");
        StringAssert.Contains(timeSpanModule.Content, "function getFiniteDoubleRatio(value)");
        StringAssert.Contains(timeSpanModule.Content, "function createFromRoundedRationalTicks(numerator, denominator)");
        StringAssert.Contains(timeSpanModule.Content, "return createFromTruncatedTicks(value * 864000000000);");
        StringAssert.Contains(timeSpanModule.Content, "let ratio = getFiniteDoubleRatio(factor);");
        StringAssert.Contains(timeSpanModule.Content, "return createFromRoundedRationalTicks(instance.ticks * ratio[0], ratio[1]);");
        StringAssert.Contains(timeSpanModule.Content, "let ratio = getFiniteDoubleRatio(divisor);");
        StringAssert.Contains(timeSpanModule.Content, "return createFromRoundedRationalTicks(numerator, denominator);");
        Assert.IsFalse(timeSpanModule.Content.Contains("let rounded = Math.round(value);", StringComparison.Ordinal), timeSpanModule.Content);

        var timeOnlyModule = modules.Single(module => string.Equals(module.RelativePath, "System/TimeOnlyModule.js", StringComparison.OrdinalIgnoreCase));
        StringAssert.Contains(timeOnlyModule.Content, "function createTruncatedTicksFromDouble(value)");
        StringAssert.Contains(timeOnlyModule.Content, "return BigInt(Math.trunc(value));");
        Assert.IsFalse(timeOnlyModule.Content.Contains("function createRoundedTicksFromDouble(value)", StringComparison.Ordinal), timeOnlyModule.Content);

        var dateTimeModule = modules.Single(module => string.Equals(module.RelativePath, "System/DateTimeModule.js", StringComparison.OrdinalIgnoreCase));
        StringAssert.Contains(dateTimeModule.Content, "function createAddUnitTicks(value, ticksPerUnit)");
        StringAssert.Contains(dateTimeModule.Content, "let integralPart = Math.trunc(value);");
        StringAssert.Contains(dateTimeModule.Content, "let fractionalPart = value - integralPart;");
        StringAssert.Contains(dateTimeModule.Content, "BigInt(Math.trunc(fractionalPart * Number(ticksPerUnit)))");
        Assert.IsFalse(dateTimeModule.Content.Contains("createRoundedTicksFromDouble(value * 864000000000)", StringComparison.Ordinal), dateTimeModule.Content);

        var dateTimeOffsetModule = modules.Single(module => string.Equals(module.RelativePath, "System/DateTimeOffsetModule.js", StringComparison.OrdinalIgnoreCase));
        StringAssert.Contains(dateTimeOffsetModule.Content, "function createAddUnitTicks(value, ticksPerUnit)");
        StringAssert.Contains(dateTimeOffsetModule.Content, "let integralPart = Math.trunc(value);");
        StringAssert.Contains(dateTimeOffsetModule.Content, "let fractionalPart = value - integralPart;");
        StringAssert.Contains(dateTimeOffsetModule.Content, "BigInt(Math.trunc(fractionalPart * Number(ticksPerUnit)))");
        Assert.IsFalse(dateTimeOffsetModule.Content.Contains("createRoundedTicksFromDouble(days * 864000000000)", StringComparison.Ordinal), dateTimeOffsetModule.Content);
    }

    private static void AssertContainsModule(IReadOnlyList<EmitModuleRecord> modules, string relativePath)
    {
        var module = modules.SingleOrDefault(module => string.Equals(module.RelativePath, relativePath, StringComparison.OrdinalIgnoreCase));

        Assert.IsNotNull(module, $"Expected CLR runtime catalog to contain '{relativePath}'.");
        Assert.AreEqual("ECMAScript", module.AssemblyName);
        Assert.IsFalse(string.IsNullOrWhiteSpace(module.TypeName), $"Expected '{relativePath}' to have a type name.");
        Assert.IsFalse(string.IsNullOrWhiteSpace(module.Content), $"Expected '{relativePath}' to have emitted module content.");
        Assert.IsFalse(string.IsNullOrWhiteSpace(module.Hash), $"Expected '{relativePath}' to have a content hash.");
    }
}
