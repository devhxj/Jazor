using Acornima;
using Jazor.RazorVue.RazorSdk;

namespace Jazor.RazorVue.Sg.Test;

[TestClass]
public sealed class VueModuleIntegrityValidatorTests
{
    [TestMethod]
    public void ValidModule_DistinguishesBindingsAndPropertyKeys()
    {
        var module = new Parser().ParseModule(
            "import { h as render } from \"vue\"; const label = \"ok\"; function view(value) { return render({ label: value, [label]: value }); } export default view;");

        VueModuleIntegrityValidator.Validate(module);
        Assert.IsEmpty(VueModuleIntegrityValidator.FindUnboundIdentifiers(module));
    }

    [TestMethod]
    public void MissingRuntimeReference_IsReportedByName()
    {
        var module = new Parser().ParseModule(
            "const view = () => missingHelper; export default view;");

        var unbound = VueModuleIntegrityValidator.FindUnboundIdentifiers(module);

        CollectionAssert.AreEquivalent(new[] { "missingHelper" }, unbound.ToArray());
        var exception = Assert.Throws<InvalidOperationException>(() => VueModuleIntegrityValidator.Validate(module));
        StringAssert.Contains(exception.Message, "missingHelper", StringComparison.Ordinal);
    }

    [TestMethod]
    public void NestedParameter_DoesNotMaskAnUnboundOuterReference()
    {
        var module = new Parser().ParseModule(
            "const view = (value) => (() => value)(); const other = missingOuter; export default view;");

        CollectionAssert.AreEquivalent(new[] { "missingOuter" }, VueModuleIntegrityValidator.FindUnboundIdentifiers(module).ToArray());
    }

    [TestMethod]
    public void ControlFlowLabels_AreNotValueReferences()
    {
        var module = new Parser().ParseModule(
            "const view = () => { outer: for (var index = 0; index < 2; index++) { if (index) break outer; } return index; }; export default view;");

        VueModuleIntegrityValidator.Validate(module);
        Assert.IsEmpty(VueModuleIntegrityValidator.FindUnboundIdentifiers(module));
    }

    [TestMethod]
    public void VarBindings_HoistAcrossNestedBlocksAndLoops()
    {
        var module = new Parser().ParseModule(
            "const view = () => { if (true) { var branchValue = 1; } while (branchValue) { var loopValue = branchValue; break; } return loopValue; }; export default view;");

        VueModuleIntegrityValidator.Validate(module);
        Assert.IsEmpty(VueModuleIntegrityValidator.FindUnboundIdentifiers(module));
    }

    [TestMethod]
    public void BlockBindings_DoNotLeakOutward()
    {
        var module = new Parser().ParseModule(
            "const view = () => { { let innerValue = 1; } return innerValue; }; export default view;");

        CollectionAssert.AreEquivalent(
            new[] { "innerValue" },
            VueModuleIntegrityValidator.FindUnboundIdentifiers(module).ToArray());
    }

    [TestMethod]
    public void NamedDefaultDeclarations_KeepTheirRecursiveBindings()
    {
        var module = new Parser().ParseModule(
            "export default function view() { return view; } export class Runtime { static { var localValue = 1; } static create() { return Runtime; } }");

        VueModuleIntegrityValidator.Validate(module);
        Assert.IsEmpty(VueModuleIntegrityValidator.FindUnboundIdentifiers(module));
    }

    [TestMethod]
    public void BrowserGlobals_AreAllowedButUnknownNamesRemainBounded()
    {
        var module = new Parser().ParseModule(
            "const view = () => window.document.querySelector(location.href) ?? navigator.userAgent; export default view;");

        VueModuleIntegrityValidator.Validate(module);
        Assert.IsEmpty(VueModuleIntegrityValidator.FindUnboundIdentifiers(module));

        var unknown = new Parser().ParseModule(
            "const view = () => window.document.querySelector(unknownBrowserGlobal); export default view;");

        CollectionAssert.AreEquivalent(
            new[] { "unknownBrowserGlobal" },
            VueModuleIntegrityValidator.FindUnboundIdentifiers(unknown).ToArray());
    }

    [TestMethod]
    public void StructuralSyntax_TracksBindingsAcrossDeclarationsExportsAndControlFlow()
    {
        var cases = new (string Source, string[] ExpectedUnbound)[]
        {
            (
                "const { [key]: value = fallback, ...rest } = source;",
                ["key", "fallback", "source"]),
            (
                "function render({ [key]: value = fallback, ...rest }) { return source; }",
                ["key", "fallback", "source"]),
            (
                "const render = function named(unused = fallback) { return source; };",
                ["fallback", "source"]),
            (
                "const render = ([unused = fallback, ...rest]) => source;",
                ["fallback", "source"]),
            (
                "try { work(); } catch ({ [key]: message = fallback, ...rest }) { report(message); }",
                ["work", "key", "fallback", "report"]),
            (
                "try { work(); } catch { report(); }",
                ["work", "report"]),
            (
                "class Widget extends Base { render(unused) { return source; } [methodName](unused = fallback) { return other; } field = initial; [fieldName] = computedInitial; }",
                ["Base", "source", "methodName", "fallback", "other", "initial", "fieldName", "computedInitial"]),
            (
                "const ctor = class Internal extends Base { method() { return source; } };",
                ["Base", "source"]),
            (
                "class Runtime { static { var localValue = source; } static create() { return localValue; } }",
                ["source", "localValue"]),
            (
                "outer: while (condition) { if (stop) break outer; continue outer; }",
                ["condition", "stop"]),
            (
                "import defaultBinding, * as namespaceBinding from \"module\"; import { named as localName } from \"module\"; export { localName }; export { remote as external } from \"module\"; export * from \"module\"; defaultBinding; namespaceBinding; localName;",
                []),
            (
                "export const local = source; export function named() { return local; } export class NamedClass { } export default function defaultView() { return named; }",
                ["source"]),
            (
                "function loops() { for (var loopKey in source) { var fromIn = loopKey; } for (var loopItem of source) { var fromOf = loopItem; } for (let lexical = 0; lexical < limit; lexical += step) { let body = lexical; } for (;;) { break; } switch (selector) { case caseValue: var fromCase = caseValue; break; default: var defaultValue = 1; } return fromIn + fromOf + fromCase + defaultValue; }",
                ["source", "limit", "step", "selector", "caseValue"]),
            (
                "function loops() { for (target in source) { target; } for (otherTarget of otherSource) { otherTarget; } }",
                ["target", "source", "otherTarget", "otherSource"]),
            (
                "function nested() { if (condition) { var branchValue = source; } do { var doValue = branchValue; } while (repeat); while (again) { var whileValue = doValue; break; } return whileValue; }",
                ["condition", "source", "repeat", "again"])
        };

        foreach (var @case in cases)
        {
            var module = new Parser().ParseModule(@case.Source);
            var unbound = VueModuleIntegrityValidator.FindUnboundIdentifiers(module);

            CollectionAssert.AreEquivalent(
                @case.ExpectedUnbound,
                unbound.ToArray(),
                @case.Source);
        }
    }

    [TestMethod]
    public void StructuralSyntax_CoversDefaultClassesBindingPatternsAndHoistedControlFlow()
    {
        var cases = new (string Source, string[] ExpectedUnbound)[]
        {
            (
                "export default class DefaultView extends Base { [methodKey](value = fallback) { return source; } [fieldKey] = initial; }",
                ["Base", "methodKey", "fallback", "source", "fieldKey", "initial"]),
            (
                "function bindings({ [key]: value = fallback, shorthand = shortFallback, ...rest }) { return value + shorthand + rest.length; }",
                ["key", "fallback", "shortFallback"]),
            (
                "function hoisted() { label: { var fromLabel = source; } try { var fromTry = fromLabel; } catch (error) { var fromCatch = error; } finally { var fromFinally = fromTry; } switch (choice) { default: var fromSwitch = fromFinally; } return fromCatch + fromSwitch; }",
                ["source", "choice"]),
            (
                "const value = ({ [key]: target = fallback, ...rest } = source);",
                ["key", "fallback", "rest", "source"]),
            (
                "const value = ([first = fallback, ...rest] = source);",
                ["fallback", "rest", "source"])
        };

        foreach (var @case in cases)
        {
            var module = new Parser().ParseModule(@case.Source);
            CollectionAssert.AreEquivalent(
                @case.ExpectedUnbound,
                VueModuleIntegrityValidator.FindUnboundIdentifiers(module).ToArray(),
                @case.Source);
        }
    }
}
