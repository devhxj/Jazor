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
}
