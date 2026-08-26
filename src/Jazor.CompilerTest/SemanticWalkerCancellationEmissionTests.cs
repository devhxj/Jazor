using Acornima;
using Jazor.Compiler;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Operations;

namespace Jazor.ComplierTest;

/// <summary>
/// 取消链（CancellationToken / CancellationTokenSource / CancellationTokenRegistration）在使用点的发射契约。
/// </summary>
/// <remarks>
/// 这三个类型分别擦除为 AbortSignal / AbortController / 内部 registration carrier，因此断言的是
/// 「CLR 成员落在哪个宿主成员或哪个模块导出上」：名称改写走 Alias，稳定单表达式走 Inline，
/// 需要模块级状态（never-abort 单例、延迟取消定时器表、listener 撤销表）的一律走 Import。
/// 走 Import 的成员必须出现为对导出名的调用，否则说明模块级状态被复制到了调用点。
/// </remarks>
[TestClass]
public sealed class SemanticWalkerCancellationEmissionTests
{
    [TestMethod]
    // source/token 的分工与 controller/signal 一一对应，访问器是纯名称改写。
    [DataRow("var value = source.Token;", "let value = source.signal;")]
    [DataRow("source.Cancel();", "source.abort();")]
    [DataRow("var value = token.IsCancellationRequested;", "let value = token.aborted;")]
    [DataRow("var value = registration.Token;", "let value = registration.signal;")]
    // 稳定单表达式模板。
    [DataRow("var value = new System.Threading.CancellationTokenSource();", "let value = new AbortController;")]
    [DataRow("var value = source.IsCancellationRequested;", "let value = source.signal.aborted;")]
    [DataRow("source.Cancel(true);", "source.abort();")]
    [DataRow("var value = source.CancelAsync();", "let value = Promise.resolve(source.abort());")]
    // 引用身份：擦除后同一个 source 就是同一个 signal / 同一个 carrier。
    [DataRow("var value = token.Equals(other);", "let value = token === other;")]
    [DataRow("var value = token == other;", "let value = token === other;")]
    [DataRow("var value = token != other;", "let value = token !== other;")]
    public void Visit_CancellationMember_LowersToHostSurface(string statements, string expected)
    {
        var script = Emit(statements);

        StringAssert.Contains(script, expected, StringComparison.Ordinal);
    }

    [TestMethod]
    // 身份单例：None / default / new CancellationToken() 必须落在同一个模块导出上，
    // 否则 default(CancellationToken) == CancellationToken.None 的 CLR 约定会破裂。
    [DataRow("var value = System.Threading.CancellationToken.None;", "getNone(")]
    [DataRow("var value = default(System.Threading.CancellationToken);", "createDefaultToken(")]
    [DataRow("var value = new System.Threading.CancellationToken();", "createDefaultToken(")]
    [DataRow("var value = new System.Threading.CancellationToken(true);", "createToken(")]
    [DataRow("var value = token.CanBeCanceled;", "getCanBeCanceled(")]
    [DataRow("token.ThrowIfCancellationRequested();", "throwIfCancellationRequested(")]
    // 注册/撤销共享同一张 listener 表。
    [DataRow("var value = token.Register(() => { });", "register(")]
    [DataRow("var value = token.UnsafeRegister((state) => { }, null);", "unsafeRegisterWithState(")]
    [DataRow("var value = registration.Unregister();", "unregister(")]
    [DataRow("registration.Dispose();", "dispose(")]
    // 延迟取消共享同一张定时器表。
    [DataRow("var value = new System.Threading.CancellationTokenSource(1000);", "createWithMillisecondsDelay(")]
    [DataRow("source.CancelAfter(1000);", "cancelAfter(")]
    [DataRow("source.Dispose();", "dispose(")]
    // 链接 source 需要新建 controller 并转发聚合信号，无法压成单表达式。
    [DataRow(
        "var value = System.Threading.CancellationTokenSource.CreateLinkedTokenSource(token);",
        "createLinkedTokenSource(")]
    [DataRow(
        "var value = System.Threading.CancellationTokenSource.CreateLinkedTokenSource(token, other);",
        "createLinkedTokenSourceFromPair(")]
    public void Visit_CancellationSharedStateMember_CallsTheModuleExport(string statements, string expected)
    {
        var script = Emit(statements);

        StringAssert.Contains(script, expected, StringComparison.Ordinal);
    }

    [TestMethod]
    // WaitHandle 是 CLR 内核同步对象；abort 是单向终态，没有复位入口；
    // 空注册会把 carrier 的 signal 变成可空，从而给撤销路径引入一个仅为占位值存在的分支。
    [DataRow("var value = token.WaitHandle;")]
    [DataRow("var value = source.TryReset();")]
    [DataRow("var value = default(System.Threading.CancellationTokenRegistration);")]
    public void Visit_UnsupportedCancellationMember_Throws(string statements)
    {
        Assert.ThrowsExactly<OperationTransformationException>(() => Emit(statements));
    }

    /// <summary>
    /// 取消链各类型的 <c>GetHashCode()</c> 保持 unsupported，因此落在 object 的身份哈希上。
    /// </summary>
    /// <remarks>
    /// signal/carrier 没有自己的数值身份，模块里发射任何近似值都会与"引用相等即语义相等"的
    /// Equals 规则冲突；退回共享的 object 身份哈希恰好与引用同一性一致，因此这是有意保留的行为，
    /// 不是漏掉的映射。
    /// </remarks>
    [TestMethod]
    [DataRow("var value = token.GetHashCode();", "_97891de43f43ceb4(token)")]
    [DataRow("var value = registration.GetHashCode();", "_97891de43f43ceb4(registration)")]
    public void Visit_CancellationHashCode_FallsBackToObjectIdentityHash(string statements, string expected)
    {
        var script = Emit(statements);

        StringAssert.Contains(script, expected, StringComparison.Ordinal);
    }

    private static string Emit(string statements)
    {
        var block = GetBlockOperation($@"
            using System;
            using System.Threading;

            public class Probe
            {{
                public void Evaluate(
                    CancellationTokenSource source,
                    CancellationToken token,
                    CancellationToken other,
                    CancellationTokenRegistration registration)
                {{
                    {statements}
                }}
            }}
            ");

        var script = new SemanticWalker(true).Visit(block, new SenseArgument())?.ToKnRECMAScript();
        Assert.IsNotNull(script);
        _ = new Parser().ParseScript("function verify(source, token, other, registration) " + script);
        return script;
    }

    private static IBlockOperation GetBlockOperation(string source)
    {
        var syntaxTree = CSharpSyntaxTree.ParseText(source, TestMetadataReferences.PreviewParseOptions);
        var compilation = CSharpCompilation.Create(
            "CancellationEmissionScenarios",
            [syntaxTree],
            TestMetadataReferences.Net11
                .Add(MetadataReference.CreateFromFile(typeof(ECMAScript.Global).Assembly.Location))
                .Add(MetadataReference.CreateFromFile(typeof(ECMAScript.ECMAScriptAttribute).Assembly.Location)),
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
