namespace Jazor.RazorVue.Sg.Test;

[TestClass]
public sealed class RazorSgFeedbackBigIntJsonRuntimeTests
{
    [TestMethod]
    public async Task TypedReplacer_NestedLongAndUlong_PreserveExactDecimalStrings()
    {
        var observation = await RazorSgOfficialAuthoringTestHost.BuildComponentAsync(
            RazorSgTestHost.GetTestDocumentPath("Pages/BigIntJsonFeedback.razor"),
            "<output>@Serialize()</output>",
            """
            namespace Demo.Pages;
            [ECMAScriptModule("./components/bigint-json-feedback")]
            public partial class BigIntJsonFeedback : ComponentBase, IVueComponent
            {
                private string Serialize()
                {
                    var payload = new ECMAScript.Vue.VueDictionary<ECMAScript.Vue.VueValue>
                    {
                        ["Id"] = 9007199254740993L,
                        ["UnsignedId"] = 18446744073709551615UL,
                        ["Count"] = 42,
                        ["Fraction"] = 1.25,
                        ["Nested"] = new ECMAScript.Vue.VueDictionary<ECMAScript.Vue.VueValue> { ["Id"] = -9007199254740993L },
                        ["Items"] = new ECMAScript.Vue.VueValue[] { 9007199254740993L, long.MaxValue, long.MinValue },
                        ["Values"] = new ECMAScript.Vue.VueValue?[] { null, 7, 9007199254740993L, "unchanged", true }
                    };
                    // Apply policy per call: BigInt IDs become decimal strings; ordinary values retain their JSON shape.
                    return JSON.Stringify(payload,
                        (key, value) => ECMAScript.Global.TypeOf(value) == "bigint" ? value!.ToString() : value)!;
                }
            }
            """,
            "Demo.Pages", "Demo.Pages.BigIntJsonFeedback");

        StringAssert.Contains(observation.ModuleText, "JSON.stringify");
        StringAssert.Contains(observation.ModuleText, "typeof value");
        await RazorSgOfficialDenoRuntimeTestHost.RunModuleTestAsync(
            "components/bigint-json-feedback.js", observation.ModuleText, "bigint-json-feedback.test.mjs",
            """
            import assert from "node:assert/strict";
            import component from "./components/bigint-json-feedback.js";
            const output = component.setup({}, { slots: {} })();
            const json = Array.isArray(output.children) ? output.children.join("") : output.children;
            const payload = JSON.parse(json);
            assert.equal(payload.Id, "9007199254740993");
            assert.equal(payload.UnsignedId, "18446744073709551615");
            assert.equal(payload.Nested.Id, "-9007199254740993");
            assert.deepEqual(payload.Items, ["9007199254740993", "9223372036854775807", "-9223372036854775808"]);
            assert.deepEqual(payload.Values, [null, 7, "9007199254740993", "unchanged", true]);
            assert.equal(payload.Count, 42);
            assert.equal(payload.Fraction, 1.25);
            // The recipe leaves the host default and process-global prototype untouched.
            assert.equal(Object.hasOwn(BigInt.prototype, "toJSON"), false);
            assert.throws(() => JSON.stringify({ id: 9007199254740993n }), TypeError);
            """);
    }
}
