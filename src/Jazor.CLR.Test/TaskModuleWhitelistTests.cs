using System.Reflection;
using ECMAScript.Contract;

namespace Jazor.CLR.Test;

[TestClass]
public sealed class TaskModuleWhitelistTests
{
    [TestMethod]
    public void TaskTypeAlias_IsPromise()
    {
        var attribute = typeof(Jazor.CLR.TaskModule).GetCustomAttribute<JazorAttribute>();

        Assert.IsNotNull(attribute);
        Assert.AreEqual(Op.Alias, attribute.Op);
        Assert.AreEqual("System.Threading.Tasks.Task", attribute.Member);
        Assert.AreEqual("Promise", attribute.Value);
    }

    [TestMethod]
    public void TaskOfTTypeAlias_IsPromise()
    {
        var attribute = typeof(Jazor.CLR.TaskT1Module<>).GetCustomAttribute<JazorAttribute>();

        Assert.IsNotNull(attribute);
        Assert.AreEqual(Op.Alias, attribute.Op);
        Assert.AreEqual("System.Threading.Tasks.Task<TResult>", attribute.Member);
        Assert.AreEqual("Promise", attribute.Value);
    }

    [TestMethod]
    public void TaskMethodMappings_DoNotUseDiscard()
    {
        var mappings = GetTaskMethodMappings().Values.ToArray();

        Assert.IsTrue(mappings.Length > 0, "Expected TaskModule to expose whitelist mappings.");
        foreach (var mapping in mappings)
        {
            Assert.AreNotEqual(Op.Discard, mapping.Op, $"Task mapping should not fallback to Discard: {mapping.Member}");
        }
    }

    [TestMethod]
    public void TaskCriticalSignatures_AreMappedToExpectedInline()
    {
        var mappings = GetTaskMethodMappings();

        AssertInlineContains(
            mappings,
            "System.Threading.Tasks.Task.AsyncState.get",
            "__jazorTaskAsyncStates?.get(__arg1)");
        AssertInlineContains(
            mappings,
            "System.Threading.Tasks.Task.Task(System.Action)",
            "__jazorTaskStarters ??= new WeakMap");
        AssertInlineContains(
            mappings,
            "System.Threading.Tasks.Task.Task(System.Action)",
            "Promise.resolve().then(() => __arg1()).then(resolve, reject)");
        AssertInlineContains(
            mappings,
            "System.Threading.Tasks.Task.Task(System.Action<object>, object)",
            "__jazorTaskAsyncStates ??= new WeakMap");
        AssertInlineContains(
            mappings,
            "System.Threading.Tasks.Task.Start()",
            "__jazorTaskStarters?.get(__arg1)");
        AssertInlineContains(
            mappings,
            "System.Threading.Tasks.Task.Start()",
            "entry && entry.start");
        AssertInlineContains(
            mappings,
            "System.Threading.Tasks.Task.Status.get",
            "s.status === \"created\" ? 0");
        AssertInlineContains(
            mappings,
            "System.Threading.Tasks.Task.Status.get",
            "const starterEntry = globalThis.__jazorTaskStarters?.get(task)");
        AssertInlineContains(
            mappings,
            "System.Threading.Tasks.Task.IsCompleted.get",
            "s.status === \"fulfilled\" || s.status === \"rejected\"");
        AssertInlineContains(
            mappings,
            "static System.Threading.Tasks.Task.CompletedTask.get",
            "Promise.resolve()");
        AssertInlineContains(
            mappings,
            "static System.Threading.Tasks.Task.Yield()",
            "Promise.resolve()");
        AssertInlineContains(
            mappings,
            "System.Threading.Tasks.Task.WaitAsync(System.TimeSpan)",
            "Promise.race([Promise.resolve(__arg1),");
        AssertInlineContains(
            mappings,
            "static System.Threading.Tasks.Task.WhenEach(params System.Threading.Tasks.Task[])",
            "async function*()");
        AssertInlineContains(
            mappings,
            "static System.Threading.Tasks.Task.WhenAny(params System.Threading.Tasks.Task[])",
            "Promise.race(Array.from(__arg1).map((task) =>");
        AssertInlineContains(
            mappings,
            "static System.Threading.Tasks.Task.FromCanceled(System.Threading.CancellationToken)",
            "Promise.reject(new Error(\"TaskCanceledException\"))");
    }

    [TestMethod]
    public void TaskGenericSignatures_ResultDiscard_AndAwaitMembersInline()
    {
        var mappings = GetTaskGenericMethodMappings();

        Assert.IsTrue(mappings.TryGetValue("System.Threading.Tasks.Task<TResult>.Result.get", out var resultMapping));
        Assert.AreEqual(Op.Discard, resultMapping.Op);

        AssertInlineContains(
            mappings,
            "System.Threading.Tasks.Task<TResult>.GetAwaiter()",
            "Promise.resolve(__arg1)");
        AssertInlineContains(
            mappings,
            "System.Threading.Tasks.Task<TResult>.ConfigureAwait(bool)",
            "Promise.resolve(__arg1)");
        AssertInlineContains(
            mappings,
            "System.Threading.Tasks.Task<TResult>.ConfigureAwait(System.Threading.Tasks.ConfigureAwaitOptions)",
            "Promise.resolve(__arg1)");
        AssertInlineContains(
            mappings,
            "System.Threading.Tasks.Task<TResult>.WaitAsync(System.Threading.CancellationToken)",
            "Promise.resolve(__arg1)");
        AssertInlineContains(
            mappings,
            "System.Threading.Tasks.Task<TResult>.WaitAsync(System.TimeSpan)",
            "Promise.race([Promise.resolve(__arg1),");
        AssertInlineContains(
            mappings,
            "System.Threading.Tasks.Task<TResult>.WaitAsync(System.TimeSpan, System.TimeProvider)",
            "Promise.race([Promise.resolve(__arg1),");
        AssertInlineContains(
            mappings,
            "System.Threading.Tasks.Task<TResult>.WaitAsync(System.TimeSpan, System.Threading.CancellationToken)",
            "Promise.race([Promise.resolve(__arg1),");
        AssertInlineContains(
            mappings,
            "System.Threading.Tasks.Task<TResult>.WaitAsync(System.TimeSpan, System.TimeProvider, System.Threading.CancellationToken)",
            "Promise.race([Promise.resolve(__arg1),");
    }

    private static IReadOnlyDictionary<string, JazorAttribute> GetTaskMethodMappings()
    {
        return typeof(Jazor.CLR.TaskModule)
            .GetMethods(BindingFlags.Public | BindingFlags.Static)
            .Select(method => method.GetCustomAttribute<JazorAttribute>())
            .OfType<JazorAttribute>()
            .ToDictionary(attribute => attribute.Member, StringComparer.Ordinal);
    }

    private static IReadOnlyDictionary<string, JazorAttribute> GetTaskGenericMethodMappings()
    {
        return typeof(Jazor.CLR.TaskT1Module<>)
            .GetMethods(BindingFlags.Public | BindingFlags.Static)
            .Select(method => method.GetCustomAttribute<JazorAttribute>())
            .OfType<JazorAttribute>()
            .ToDictionary(attribute => attribute.Member, StringComparer.Ordinal);
    }

    private static void AssertInlineContains(
        IReadOnlyDictionary<string, JazorAttribute> mappings,
        string member,
        string expectedSnippet)
    {
        Assert.IsTrue(mappings.TryGetValue(member, out var mapping), $"Missing Task mapping: {member}");
        Assert.AreEqual(Op.Inline, mapping.Op, $"Task mapping should be Inline: {member}");
        Assert.IsNotNull(mapping.Value, $"Task mapping value should not be null: {member}");
        StringAssert.Contains(mapping.Value, expectedSnippet, StringComparison.Ordinal);
    }
}
