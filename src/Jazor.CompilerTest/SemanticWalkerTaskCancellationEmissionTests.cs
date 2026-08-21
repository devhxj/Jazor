using Acornima;
using Jazor.Compiler;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Operations;

namespace Jazor.ComplierTest;

/// <summary>
/// Task 家族接受 <c>CancellationToken</c> 的重载在使用点必须真的把 token 接进模板。
/// </summary>
/// <remarks>
/// 这些重载擦除后都落在同一个 Promise carrier 上，因此"token 被忽略"不会有任何编译期或运行时报错，
/// 只会静默退化成不可取消的等待。断言固定的是三段共享语义各自的稳定标记：
/// <list type="bullet">
/// <item>竞速取消走 <c>__jazorTaskWithCancellation</c>，并按失败协议区分 Task 面与阻塞 Wait 面的取消原因；</item>
/// <item>延时取消走 <c>__jazorTaskDelay</c>，它是唯一必须撤掉定时器的路径；</item>
/// <item>冷启动 Task 与 <c>Task.Run</c> 只在"尚未开始执行"时受 token 影响。</item>
/// </list>
/// 模板文本本身由 <c>Jazor.CLR.Test</c> 的白名单断言固定，这里只验证使用点确实把 token 实参
/// 传到了模板的取消位上。
/// </remarks>
[TestClass]
public sealed class SemanticWalkerTaskCancellationEmissionTests
{
    /// <summary>
    /// 阻塞式 Wait / WaitAll / WaitAny 在 CLR 下直接抛 <c>OperationCanceledException</c>，不产出 Task。
    /// </summary>
    [TestMethod]
    [DataRow("task.Wait(token);")]
    [DataRow("var value = task.Wait(delay, token);")]
    [DataRow("var value = task.Wait(1000, token);")]
    [DataRow("System.Threading.Tasks.Task.WaitAll(tasks, token);")]
    [DataRow("System.Threading.Tasks.Task.WaitAll(sequence, token);")]
    [DataRow("var value = System.Threading.Tasks.Task.WaitAll(tasks, 1000, token);")]
    [DataRow("var value = System.Threading.Tasks.Task.WaitAny(tasks, token);")]
    [DataRow("var value = System.Threading.Tasks.Task.WaitAny(tasks, 1000, token);")]
    public void Visit_BlockingWaitWithToken_RacesAgainstTheOperationCanceledReason(string statements)
    {
        var script = Emit(statements);

        StringAssert.Contains(script, "__jazorTaskWithCancellation", StringComparison.Ordinal);
        StringAssert.Contains(
            script,
            "token, \"OperationCanceledException: The operation was canceled.\"",
            StringComparison.Ordinal);
    }

    /// <summary>
    /// 产出 Task 的取消路径必须落在 <c>"TaskCanceledException"</c> 这个精确载荷上：
    /// Status / IsCanceled / IsFaulted 按该字符串识别取消。
    /// </summary>
    [TestMethod]
    [DataRow("var value = task.WaitAsync(token);")]
    [DataRow("var value = task.WaitAsync(delay, token);")]
    [DataRow("var value = typed.WaitAsync(token);")]
    [DataRow("var value = typed.WaitAsync(delay, token);")]
    [DataRow("var value = task.ContinueWith(continuation, token);")]
    [DataRow("var value = task.ContinueWith(statefulContinuation, state, token);")]
    public void Visit_TaskProducingCancellation_RacesAgainstTheTaskCanceledReason(string statements)
    {
        var script = Emit(statements);

        StringAssert.Contains(script, "__jazorTaskWithCancellation", StringComparison.Ordinal);
        StringAssert.Contains(script, "token, \"TaskCanceledException\"", StringComparison.Ordinal);
    }

    /// <summary>
    /// 竞速取消必须在被等待方先落定时撤下 listener。
    /// </summary>
    /// <remarks>
    /// <c>default(CancellationToken)</c> 与 <c>CancellationToken.None</c> 共用同一个 never-abort 单例，
    /// 不撤销会让 listener 在这个全局单例上跨调用无上限累积。
    /// </remarks>
    [TestMethod]
    public void Visit_TaskCancellationWrapper_RemovesTheAbortListenerWhenTheAwaitedSideSettles()
    {
        var script = Emit("var value = task.WaitAsync(token);");

        StringAssert.Contains(script, "removeEventListener(\"abort\", fail", StringComparison.Ordinal);
    }

    /// <summary>
    /// Delay 是唯一必须撤掉定时器的取消路径：定时器本身就是这次操作。
    /// </summary>
    [TestMethod]
    [DataRow("var value = System.Threading.Tasks.Task.Delay(1000, token);", "(1000, token)")]
    [DataRow(
        "var value = System.Threading.Tasks.Task.Delay(delay, token);",
        "(Number(delay.ticks / 10000n), token)")]
    public void Visit_DelayWithToken_InstallsTheTimerClearingHelper(string statements, string expected)
    {
        var script = Emit(statements);

        StringAssert.Contains(script, "__jazorTaskDelay", StringComparison.Ordinal);
        StringAssert.Contains(script, "clearTimeout(id)", StringComparison.Ordinal);
        StringAssert.Contains(script, expected, StringComparison.Ordinal);
    }

    /// <summary>
    /// <c>Task.Run</c> 的 token 只影响"尚未开始执行"的工作：进入调度微任务时若已取消就不调用委托。
    /// </summary>
    [TestMethod]
    [DataRow("var value = System.Threading.Tasks.Task.Run(work, token);")]
    public void Visit_RunWithToken_ChecksAbortedBeforeInvokingTheDelegate(string statements)
    {
        var script = Emit(statements);

        StringAssert.Contains(script, "token.aborted", StringComparison.Ordinal);
        StringAssert.Contains(script, "\"TaskCanceledException\"", StringComparison.Ordinal);
    }

    /// <summary>
    /// 实参不是简单名称时，模板会被提升进 IIFE 并按位置绑定 <c>__jz_argN</c>。
    /// </summary>
    /// <remarks>
    /// inline 模板可以多次引用同一个占位符（这里 <c>__arg1</c> 出现在 abort 分支之后），
    /// 直接文本替换会让实参被求值多次；提升成 IIFE 形参保证每个实参只求值一次，
    /// 同时 token 仍然被绑定到取消位上。
    /// </remarks>
    [TestMethod]
    public void Visit_RunWithTokenAndInlineDelegate_HoistsArgumentsIntoAnIife()
    {
        var script = Emit("var value = System.Threading.Tasks.Task.Run(() => 1, token);");

        StringAssert.Contains(script, "(__jz_arg0, __jz_arg1) =>", StringComparison.Ordinal);
        StringAssert.Contains(script, "__jz_arg1.aborted", StringComparison.Ordinal);
        StringAssert.Contains(script, "}, token);", StringComparison.Ordinal);
    }

    /// <summary>
    /// 冷启动 Task：token 取消一个尚未 Start 的 Task 时直接进入 Canceled，
    /// 因此 cancel 与 start 抢同一个 <c>entry.started</c> 闸门。
    /// </summary>
    [TestMethod]
    [DataRow("var value = new System.Threading.Tasks.Task(work, token);")]
    [DataRow("var value = new System.Threading.Tasks.Task(statefulWork, state, token);")]
    public void Visit_ColdTaskWithToken_GatesStartAgainstCancel(string statements)
    {
        var script = Emit(statements);

        StringAssert.Contains(script, "__jazorTaskStarters", StringComparison.Ordinal);
        StringAssert.Contains(script, "entry.started", StringComparison.Ordinal);
        StringAssert.Contains(script, "token.aborted", StringComparison.Ordinal);
        StringAssert.Contains(script, "token.addEventListener(\"abort\", cancel", StringComparison.Ordinal);
    }

    private static string Emit(string statements)
    {
        var block = GetBlockOperation($@"
            using System;
            using System.Collections.Generic;
            using System.Threading;
            using System.Threading.Tasks;

            public class Probe
            {{
                public void Evaluate(
                    Task task,
                    Task<int> typed,
                    Task[] tasks,
                    IEnumerable<Task> sequence,
                    Action work,
                    Action<object> statefulWork,
                    Action<Task> continuation,
                    Action<Task, object> statefulContinuation,
                    TimeSpan delay,
                    CancellationToken token,
                    object state)
                {{
                    {statements}
                }}
            }}
            ");

        var script = new SemanticWalker(true).Visit(block, new SenseArgument())?.ToKnRECMAScript();
        Assert.IsNotNull(script);
        _ = new Parser().ParseScript(
            "function verify(task, typed, tasks, sequence, work, statefulWork, continuation, "
            + "statefulContinuation, delay, token, state) " + script);
        return script;
    }

    private static IBlockOperation GetBlockOperation(string source)
    {
        var syntaxTree = CSharpSyntaxTree.ParseText(source, TestMetadataReferences.PreviewParseOptions);
        var compilation = CSharpCompilation.Create(
            "TaskCancellationEmissionScenarios",
            [syntaxTree],
            TestMetadataReferences.Net11.Add(MetadataReference.CreateFromFile(typeof(ECMAScript.Global).Assembly.Location)),
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
        var errors = compilation.GetDiagnostics()
            .Where(static diagnostic => diagnostic.Severity == DiagnosticSeverity.Error)
            .ToArray();
        Assert.HasCount(0, errors, string.Join(Environment.NewLine, errors.Select(static error => error.ToString())));

        var method = syntaxTree.GetRoot().DescendantNodes()
            .OfType<MethodDeclarationSyntax>()
            .Single(static candidate => candidate.Identifier.ValueText == "Evaluate");
        return Assert.IsInstanceOfType<IBlockOperation>(compilation.GetSemanticModel(syntaxTree).GetOperation(method.Body!));
    }
}
