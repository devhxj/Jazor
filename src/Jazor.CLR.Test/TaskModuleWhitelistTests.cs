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

    /// <summary>
    /// 接受 <c>CancellationToken</c> 的 Task 重载必须把 token 接进模板的取消位。
    /// </summary>
    /// <remarks>
    /// token 被忽略不会有任何编译期或运行时报错，只会静默退化成不可取消的等待，因此这里逐个重载
    /// 固定「token 落在第几个占位符上」以及取消原因的精确载荷：产出 Task 的路径用
    /// <c>"TaskCanceledException"</c>（Status / IsCanceled / IsFaulted 按该字符串识别取消），
    /// 阻塞式 Wait 面用运行时统一的 <c>"&lt;ExceptionName&gt;: &lt;message&gt;"</c> 失败格式。
    /// </remarks>
    [TestMethod]
    public void TaskCancellationOverloads_RaceAgainstTheToken()
    {
        var mappings = GetTaskMethodMappings();

        AssertInlineContains(mappings, "System.Threading.Tasks.Task.Wait(System.Threading.CancellationToken)", CanceledWait("__arg2"));
        AssertInlineContains(mappings, "System.Threading.Tasks.Task.Wait(System.TimeSpan, System.Threading.CancellationToken)", CanceledWait("__arg3"));
        AssertInlineContains(mappings, "System.Threading.Tasks.Task.Wait(int, System.Threading.CancellationToken)", CanceledWait("__arg3"));
        AssertInlineContains(mappings, "static System.Threading.Tasks.Task.WaitAll(System.Threading.Tasks.Task[], System.Threading.CancellationToken)", CanceledWait("__arg2"));
        AssertInlineContains(mappings, "static System.Threading.Tasks.Task.WaitAll(System.Threading.Tasks.Task[], int, System.Threading.CancellationToken)", CanceledWait("__arg3"));
        AssertInlineContains(mappings, "static System.Threading.Tasks.Task.WaitAny(System.Threading.Tasks.Task[], System.Threading.CancellationToken)", CanceledWait("__arg2"));
        AssertInlineContains(mappings, "static System.Threading.Tasks.Task.WaitAny(System.Threading.Tasks.Task[], int, System.Threading.CancellationToken)", CanceledWait("__arg3"));
        // IEnumerable 重载先物化成数组再交给 Promise.all，不能直接把序列丢进去。
        AssertInlineContains(
            mappings,
            "static System.Threading.Tasks.Task.WaitAll(System.Collections.Generic.IEnumerable<System.Threading.Tasks.Task>, System.Threading.CancellationToken)",
            "Promise.all(Array.from(__arg1))");
        AssertInlineContains(
            mappings,
            "static System.Threading.Tasks.Task.WaitAll(System.Collections.Generic.IEnumerable<System.Threading.Tasks.Task>, System.Threading.CancellationToken)",
            CanceledWait("__arg2"));

        AssertInlineContains(mappings, "System.Threading.Tasks.Task.WaitAsync(System.Threading.CancellationToken)", CanceledTask("__arg2"));
        AssertInlineContains(mappings, "System.Threading.Tasks.Task.WaitAsync(System.TimeSpan, System.Threading.CancellationToken)", CanceledTask("__arg3"));
        AssertInlineContains(mappings, "System.Threading.Tasks.Task.WaitAsync(System.TimeSpan, System.TimeProvider, System.Threading.CancellationToken)", CanceledTask("__arg4"));

        // 竞速取消必须在被等待方先落定时撤下 listener：default(CancellationToken) 与 None 共用同一个
        // never-abort 单例，不撤销会让 listener 在这个全局单例上跨调用无上限累积。
        AssertInlineContains(
            mappings,
            "System.Threading.Tasks.Task.WaitAsync(System.Threading.CancellationToken)",
            "signal.removeEventListener(\"abort\", fail)");
    }

    /// <summary>
    /// 延续、冷启动与 <c>Task.Run</c> 的 token 语义各自不同，模板不能互相借用。
    /// </summary>
    /// <remarks>
    /// 延续只与「前继落定」竞速（延续一旦开始执行就不再受 token 影响）；
    /// 冷启动 Task 与 <c>Task.Run</c> 的 token 只影响「尚未开始执行」的工作；
    /// <c>Delay</c> 是唯一必须撤掉定时器的取消路径，因为定时器本身就是这次操作。
    /// </remarks>
    [TestMethod]
    public void TaskCancellationLifecycleOverloads_GateOnTheRightMoment()
    {
        var mappings = GetTaskMethodMappings();

        // 前继的成败折叠成同一个哨兵值：延续在两种情况下都要跑，只是不能吞掉 race 的拒绝。
        AssertInlineContains(
            mappings,
            "System.Threading.Tasks.Task.ContinueWith(System.Action<System.Threading.Tasks.Task>, System.Threading.CancellationToken)",
            "Promise.resolve(__arg1).then(() => 0, () => 0)" + CanceledTask("__arg3") + ".then(() => __arg2(__arg1))");
        AssertInlineContains(
            mappings,
            "System.Threading.Tasks.Task.ContinueWith(System.Action<System.Threading.Tasks.Task, object>, object, System.Threading.CancellationToken)",
            CanceledTask("__arg4") + ".then(() => __arg2(__arg1, __arg3))");

        // 进入调度微任务时若已取消就不调用委托。
        AssertInlineContains(
            mappings,
            "static System.Threading.Tasks.Task.Run(System.Action, System.Threading.CancellationToken)",
            "if (__arg2.aborted) { throw new Error(\"TaskCanceledException\"); }");

        AssertInlineContains(mappings, "static System.Threading.Tasks.Task.Delay(int, System.Threading.CancellationToken)", "clearTimeout(id)");
        AssertInlineContains(mappings, "static System.Threading.Tasks.Task.Delay(int, System.Threading.CancellationToken)", "(__arg1, __arg2)");
        AssertInlineContains(mappings, "static System.Threading.Tasks.Task.Delay(System.TimeSpan, System.Threading.CancellationToken)", "(Number(__arg1.ticks / 10000n), __arg2)");
        AssertInlineContains(mappings, "static System.Threading.Tasks.Task.Delay(System.TimeSpan, System.TimeProvider, System.Threading.CancellationToken)", "(Number(__arg1.ticks / 10000n), __arg3)");

        // cancel 与 start 抢同一个 entry.started 闸门。
        AssertInlineContains(mappings, "System.Threading.Tasks.Task.Task(System.Action, System.Threading.CancellationToken)", "__arg2.addEventListener(\"abort\", cancel");
        AssertInlineContains(mappings, "System.Threading.Tasks.Task.Task(System.Action<object>, object, System.Threading.CancellationToken)", "__arg3.addEventListener(\"abort\", cancel");
    }

    /// <summary>
    /// <c>Task&lt;TResult&gt;</c> 侧的取消重载与 <c>Task</c> 侧共用同一批模板，token 位置必须一致。
    /// </summary>
    [TestMethod]
    public void TaskOfTCancellationOverloads_RaceAgainstTheToken()
    {
        var mappings = GetTaskGenericMethodMappings();

        AssertInlineContains(mappings, "System.Threading.Tasks.Task<TResult>.WaitAsync(System.Threading.CancellationToken)", CanceledTask("__arg2"));
        AssertInlineContains(mappings, "System.Threading.Tasks.Task<TResult>.WaitAsync(System.TimeSpan, System.Threading.CancellationToken)", CanceledTask("__arg3"));
        AssertInlineContains(mappings, "System.Threading.Tasks.Task<TResult>.WaitAsync(System.TimeSpan, System.TimeProvider, System.Threading.CancellationToken)", CanceledTask("__arg4"));
    }

    private static string CanceledTask(string tokenArgument)
        => $", {tokenArgument}, \"TaskCanceledException\")";

    private static string CanceledWait(string tokenArgument)
        => $", {tokenArgument}, \"OperationCanceledException: The operation was canceled.\")";

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
