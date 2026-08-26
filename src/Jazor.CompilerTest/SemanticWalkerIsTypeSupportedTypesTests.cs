using Acornima;
using Jazor.Compiler;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Operations;

namespace Jazor.ComplierTest;

/// <summary>
/// `is` 类型判别在受支持类型面上的运行时判别契约。
/// </summary>
/// <remarks>
/// 这里固定的是「同一 CLR 类型在使用点擦除成哪个 JavaScript 判别式」这一层契约：
/// 标量走 typeof、集合走 Array.isArray/Map/Set、有 carrier 的值类型走 instanceof carrier。
/// 判别精度受擦除模型限制（类型实参、数组元素类型、共用 carrier 的类型族都不参与运行时判别），
/// 因此不可判定的检查必须显式失败，而不是发射假阳性的 instanceof。
/// </remarks>
[TestClass]
public sealed class SemanticWalkerIsTypeSupportedTypesTests
{
    [TestMethod]
    // 标量域：擦除为 JavaScript 原始类型，判别一律走 typeof。
    [DataRow("string", "typeof value === \"string\"")]
    [DataRow("char", "typeof value === \"string\"")]
    [DataRow("int", "typeof value === \"number\"")]
    [DataRow("double", "typeof value === \"number\"")]
    [DataRow("decimal", "typeof value === \"number\"")]
    [DataRow("System.Half", "typeof value === \"number\"")]
    [DataRow("long", "typeof value === \"bigint\"")]
    [DataRow("System.Int128", "typeof value === \"bigint\"")]
    [DataRow("System.Numerics.BigInteger", "typeof value === \"bigint\"")]
    [DataRow("bool", "typeof value === \"boolean\"")]
    // enum 是编译期域类型，使用点擦除为底层标量。
    [DataRow("System.DayOfWeek", "typeof value === \"number\"")]
    // 共用 JavaScript string carrier 的白名单类型。
    [DataRow("System.Guid", "typeof value === \"string\"")]
    [DataRow("System.Text.StringBuilder", "typeof value === \"string\"")]
    [DataRow("System.Globalization.CultureInfo", "typeof value === \"string\"")]
    // 有内部 carrier 的值类型：判别走 carrier 构造器。
    [DataRow("System.DateTime", "value instanceof JDateTime")]
    [DataRow("System.DateOnly", "value instanceof JDateOnly")]
    [DataRow("System.TimeOnly", "value instanceof JTimeOnly")]
    [DataRow("System.TimeSpan", "value instanceof JTimeSpan")]
    [DataRow("System.DateTimeOffset", "value instanceof JDateTimeOffset")]
    // 取消链：token/source 落在宿主的 AbortSignal/AbortController 上，registration 走生成的 carrier。
    [DataRow("System.Threading.CancellationToken", "value instanceof AbortSignal")]
    [DataRow("System.Threading.CancellationTokenSource", "value instanceof AbortController")]
    [DataRow("System.Threading.CancellationTokenRegistration", "value instanceof JCancellationTokenRegistration")]
    // 集合面：数组/List 擦除为数组，Dictionary/HashSet 擦除为 Map/Set。
    [DataRow("int[]", "Array.isArray(value)")]
    [DataRow("string[]", "Array.isArray(value)")]
    [DataRow("List<int>", "Array.isArray(value)")]
    [DataRow("Dictionary<string, int>", "value instanceof Map")]
    [DataRow("HashSet<int>", "value instanceof Set")]
    // 接口目标：只有映射到具体运行时载体的集合接口可判别。
    [DataRow("IEnumerable<int>", "Array.isArray(value)")]
    [DataRow("IDictionary<string, int>", "value instanceof Map")]
    // 映射到 JavaScript 内建类的白名单类型。
    [DataRow("System.Exception", "value instanceof Error")]
    [DataRow("System.ArgumentNullException", "value instanceof TypeError")]
    [DataRow("System.Uri", "value instanceof URL")]
    // Nullable<T> 判别等价于底层 T：装箱值要么是 null（不匹配），要么就是 T。
    [DataRow("int?", "typeof value === \"number\"")]
    [DataRow("System.Nullable<int>", "typeof value === \"number\"")]
    [DataRow("System.DateTime?", "value instanceof JDateTime")]
    // 委托只擦除为 JavaScript 函数：typeof 对「是否可调用」精确，参数个数与签名不参与判别。
    [DataRow("Action", "typeof value === \"function\"")]
    [DataRow("Action<int>", "typeof value === \"function\"")]
    [DataRow("Func<int>", "typeof value === \"function\"")]
    [DataRow("Func<int, string>", "typeof value === \"function\"")]
    [DataRow("Predicate<int>", "typeof value === \"function\"")]
    [DataRow("Handler", "typeof value === \"function\"")]
    // object 只剩非空判别；同模块类走原型链。
    [DataRow("object", "value != null")]
    [DataRow("Probe", "value instanceof Probe")]
    // 元组擦除为对象字面量，只保留结构性判别。
    [DataRow("System.ValueTuple<int, int>", "value !== null && typeof value === \"object\"")]
    public void Visit_IsType_SupportedTarget_EmitsDeterministicRuntimeDiscriminator(string target, string expected)
    {
        var script = Emit($"bool result = value is {target};");

        StringAssert.Contains(script, $"let result = {expected};", StringComparison.Ordinal);
    }

    /// <summary>
    /// Nullable 目标不得退化成对不存在的 Nullable 运行时类型做 instanceof。
    /// </summary>
    [TestMethod]
    public void Visit_IsType_NullableTarget_DoesNotReferenceNullableRuntimeType()
    {
        var script = Emit("bool result = value is int?;");

        Assert.IsFalse(script.Contains("Nullable", StringComparison.Ordinal), script);
    }

    /// <summary>
    /// 共用运行时别名的类型族：Roslyn 能证明来源时按编译期结果折叠。
    /// </summary>
    [TestMethod]
    // 精确命中：来源确定就是该类型。
    [DataRow("new System.Threading.Tasks.ValueTask()", "System.Threading.Tasks.ValueTask", "true")]
    // 证伪：来源确定不是该类型，即使运行时载体相同。
    [DataRow("new System.Threading.Tasks.ValueTask()", "System.Threading.Tasks.Task", "false")]
    // 只能证明「非空即匹配」：来源静态类型可赋值到目标，但值不是编译期常量。
    [DataRow("System.Threading.Tasks.Task.CompletedTask", "System.Threading.Tasks.Task", "source != null")]
    [DataRow("System.Threading.Tasks.Task.FromResult(1)", "System.Threading.Tasks.Task<int>", "source != null")]
    public void Visit_IsType_SharedRuntimeAliasWithProvableSource_FoldsAtCompileTime(
        string sourceExpression,
        string target,
        string expected)
    {
        var script = Emit($@"object source = {sourceExpression};
                    bool result = source is {target};");

        StringAssert.Contains(script, $"let result = {expected};", StringComparison.Ordinal);
    }

    /// <summary>
    /// 共用运行时别名且无法证明的检查必须显式失败，不允许发射假阳性的 instanceof。
    /// </summary>
    [TestMethod]
    [DataRow("System.Threading.Tasks.Task", "Promise")]
    [DataRow("System.Threading.Tasks.ValueTask", "Promise")]
    public void Visit_IsType_SharedRuntimeAliasWithUnprovableSource_Throws(string target, string runtimeAlias)
    {
        var exception = Assert.ThrowsExactly<OperationTransformationException>(
            () => Emit($"bool result = value is {target};"));

        StringAssert.Contains(exception.Message, $"runtime alias '{runtimeAlias}' is shared", StringComparison.Ordinal);
        StringAssert.Contains(exception.Message, target, StringComparison.Ordinal);
    }

    /// <summary>
    /// 不受支持的目标类型在使用点显式失败：外部未映射类型、擦除接口、无运行时判别的类型族。
    /// </summary>
    [TestMethod]
    [DataRow("System.Text.RegularExpressions.Regex", "is not supported")]
    [DataRow("System.ArgumentException", "is not supported")]
    // 异常族只映射有 JavaScript 内建对应物的类型：Exception -> Error、ArgumentNullException -> TypeError。
    // 没有对应物的异常不纳入支持范围，避免别名回 Error 后与基类共用运行时形状。
    [DataRow("System.InvalidOperationException", "is not supported")]
    [DataRow("System.DivideByZeroException", "is not supported")]
    [DataRow("System.ValueType", "is not supported")]
    [DataRow("IDisposable", "cannot be statically proven assignable")]
    [DataRow("IConvertible", "Unsupported type in is-type operation")]
    public void Visit_IsType_UnsupportedTarget_Throws(string target, string expectedMessage)
    {
        var exception = Assert.ThrowsExactly<OperationTransformationException>(
            () => Emit($"bool result = value is {target};"));

        StringAssert.Contains(exception.Message, expectedMessage, StringComparison.Ordinal);
    }

    /// <summary>
    /// object 上的位置模式依赖 ITuple 运行时协议，擦除后的元组不提供该协议，必须显式失败。
    /// </summary>
    [TestMethod]
    public void Visit_IsPattern_PositionalPatternOnErasedObject_ThrowsForITupleProtocol()
    {
        var exception = Assert.ThrowsExactly<OperationTransformationException>(
            () => Emit("bool result = value is (int, int);"));

        StringAssert.Contains(exception.Message, "ITuple", StringComparison.Ordinal);
    }

    /// <summary>
    /// 元组静态类型上的位置模式仍按擦除后的元素属性判别。
    /// </summary>
    [TestMethod]
    public void Visit_IsPattern_PositionalPatternOnTupleTyped_MatchesErasedElements()
    {
        var block = GetBlockOperation(@"
            public class Probe
            {
                public void Evaluate()
                {
                    (int, int) pair = (1, 2);
                    bool result = pair is (1, 2);
                }
            }
            ");

        var script = new SemanticWalker(true).Visit(block, new SenseArgument())?.ToKnRECMAScript();

        Assert.IsNotNull(script);
        StringAssert.Contains(script, "let result = pair.Item1 === 1 && pair.Item2 === 2;", StringComparison.Ordinal);
    }

    private static string Emit(string statements)
    {
        var block = GetBlockOperation($@"
            using System;
            using System.Collections.Generic;

            public delegate void Handler(int value);

            public class Probe
            {{
                public void Evaluate(object value)
                {{
                    {statements}
                }}
            }}
            ");

        var script = new SemanticWalker(true).Visit(block, new SenseArgument())?.ToKnRECMAScript();
        Assert.IsNotNull(script);
        _ = new Parser().ParseScript("function verify(value) " + script);
        return script;
    }

    private static IBlockOperation GetBlockOperation(string source)
    {
        var syntaxTree = CSharpSyntaxTree.ParseText(source, TestMetadataReferences.PreviewParseOptions);
        var compilation = CSharpCompilation.Create(
            "IsTypeSupportedTypeScenarios",
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
