using ECMAScript;
using Jazor.Compiler;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Operations;
using System.Reflection;
using System.Text.RegularExpressions;
using EsNode = Acornima.Ast.Node;

namespace Jazor.ComplierTest;

[TestClass]
public sealed class SemanticWalkerNotSupportTest
{
    private static IBlockOperation GetBlockOperation(string code, string methodName = "TestMethod")
    {
        var usings = @"
          global using System;
          global using System.Collections.Generic;
          global using System.Linq;
          global using System.Numerics;
          global using ECMAScript;
          global using static ECMAScript.Global;";

        var references = TestMetadataReferences.Net11
            .Add(MetadataReference.CreateFromFile(typeof(Global).Assembly.Location));
        var compilation = CSharpCompilation.Create(
            assemblyName: "TestAssembly",
            syntaxTrees: [
                CSharpSyntaxTree.ParseText(usings),
                CSharpSyntaxTree.ParseText(code)
            ],
            references: references,
            options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        var diagnostics = compilation.GetDiagnostics();
        var errors = diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error).ToList();
        if (errors.Count > 0)
        {
            var errorMessages = string.Join("\n", errors.Select(e => $"{e.Id}: {e.GetMessage()}"));
            throw new InvalidOperationException(errorMessages);
        }

        var syntaxTree = compilation.SyntaxTrees.Last();
        var semanticModel = compilation.GetSemanticModel(syntaxTree);
        var root = syntaxTree.GetRoot();
        var methodDeclaration = root.DescendantNodes()
            .OfType<MethodDeclarationSyntax>()
            .FirstOrDefault(method => method.Identifier.ValueText == methodName && method.Body is not null)
            ?? root.DescendantNodes().OfType<MethodDeclarationSyntax>().FirstOrDefault(static method => method.Body is not null);
        if (methodDeclaration?.Body is not null &&
            semanticModel.GetOperation(methodDeclaration.Body) is IBlockOperation operation)
            return operation;

        throw new InvalidOperationException("未找到可分析的操作");
    }

    private static TOperation FindFirstOperation<TOperation>(IOperation root)
        where TOperation : class, IOperation
    {
        var stack = new Stack<IOperation>();
        stack.Push(root);
        while (stack.Count > 0)
        {
            var operation = stack.Pop();
            if (operation is TOperation matched)
                return matched;

            foreach (var child in operation.ChildOperations)
            {
                if (child is not null)
                    stack.Push(child);
            }
        }

        throw new InvalidOperationException($"未找到可分析的操作 {typeof(TOperation).Name}");
    }

    private class OperationDispatchProxy : DispatchProxy
    {
        public OperationKind Kind { get; set; } = OperationKind.None;

        public SyntaxNode Syntax { get; set; } = SyntaxFactory.ParseStatement("int x = 0;");

        protected override object? Invoke(MethodInfo? targetMethod, object?[]? args)
        {
            if (targetMethod is null)
                return null;

            return targetMethod.Name switch
            {
                "get_Kind" => Kind,
                "get_Syntax" => Syntax,
                "get_ChildOperations" => Array.Empty<IOperation>(),
                _ => targetMethod.ReturnType == typeof(void)
                    ? null
                    : targetMethod.ReturnType.IsValueType
                        ? Activator.CreateInstance(targetMethod.ReturnType)
                        : null
            };
        }
    }

    private static IOperation CreateOperationProxy(Type operationInterface, OperationKind kind)
    {
        var createMethod = typeof(DispatchProxy)
            .GetMethods(BindingFlags.Public | BindingFlags.Static)
            .Single(method => method.Name == nameof(DispatchProxy.Create) && method.IsGenericMethodDefinition);
        var genericCreate = createMethod.MakeGenericMethod(operationInterface, typeof(OperationDispatchProxy));
        var instance = genericCreate.Invoke(null, null)
            ?? throw new InvalidOperationException($"无法创建代理实例: {operationInterface.FullName}");
        var proxy = instance as OperationDispatchProxy
            ?? throw new InvalidOperationException($"代理实例转换失败: {operationInterface.FullName}");
        proxy.Kind = kind;
        return (IOperation)instance;
    }

    private static string ResolveNotSupportSourcePath()
    {
        var current = new System.IO.DirectoryInfo(AppContext.BaseDirectory);
        while (current is not null)
        {
            var solutionPath = Path.Combine(current.FullName, "Jazor.slnx");
            if (System.IO.File.Exists(solutionPath))
            {
                var sourcePath = Path.Combine(current.FullName, "src", "Jazor.Compiler", "core", "SemanticWalker.cs.NotSupport.cs");
                if (System.IO.File.Exists(sourcePath))
                    return sourcePath;

                throw new InvalidOperationException($"未找到 NotSupport 源文件: {sourcePath}");
            }

            current = current.Parent;
        }

        throw new InvalidOperationException("未找到仓库根目录 Jazor.slnx。");
    }

    private static IReadOnlyList<(string MethodName, Type OperationType)> LoadNotSupportHandlers()
    {
        var sourcePath = ResolveNotSupportSourcePath();
        var source = System.IO.File.ReadAllText(sourcePath);
        var matcher = new Regex(
            @"public\s+override\s+Node\?\s+(?<method>Visit\w+)\s*\(\s*(?<op>I\w+Operation)\s+\w+\s*,\s*SenseArgument\s+\w+\s*\)",
            RegexOptions.Compiled);
        var operationAssembly = typeof(IOperation).Assembly;
        var operationTypes = operationAssembly
            .GetTypes()
            .Where(static type => type.IsInterface && typeof(IOperation).IsAssignableFrom(type))
            .GroupBy(static type => type.Name, StringComparer.Ordinal)
            .ToDictionary(group => group.Key, group => group.First(), StringComparer.Ordinal);

        var handlers = new List<(string MethodName, Type OperationType)>();
        foreach (Match match in matcher.Matches(source))
        {
            var methodName = match.Groups["method"].Value;
            var operationTypeName = match.Groups["op"].Value;
            if (!operationTypes.TryGetValue(operationTypeName, out var operationType))
                throw new InvalidOperationException($"无法解析操作类型: {operationTypeName}");
            handlers.Add((methodName, operationType));
        }

        return handlers;
    }

    private static void AssertUnsupportedDirect<TOperation>(
        IBlockOperation block,
        Func<IBlockOperation, TOperation> selector,
        Action<SemanticWalker, TOperation> visitor,
        string expectedMessage)
        where TOperation : class, IOperation
    {
        var operation = selector(block);
        var walker = new SemanticWalker(true);
        var exception = Assert.Throws<OperationTransformationException>(() => visitor(walker, operation));
        Assert.AreEqual(operation.Kind, exception.Kind);
        StringAssert.Contains(exception.Message ?? string.Empty, expectedMessage);
    }

    private static void AssertUnsupportedByDispatch(
        string code,
        OperationKind expectedKind,
        string expectedMessage,
        string methodName = "TestMethod")
    {
        var block = GetBlockOperation(code, methodName);
        var walker = new SemanticWalker(true);
        var exception = Assert.Throws<OperationTransformationException>(() => walker.Visit(block, new()));
        Assert.AreEqual(expectedKind, exception.Kind);
        StringAssert.Contains(exception.Message ?? string.Empty, expectedMessage);
    }

    [TestMethod]
    public void VisitLock_NotSupported()
    {
        const string code = """
            class TestClass
            {
                void TestMethod()
                {
                    object gate = new object();
                    lock (gate)
                    {
                        Console.WriteLine(gate);
                    }
                }
            }
            """;

        var block = GetBlockOperation(code);
        AssertUnsupportedDirect(
            block,
            static x => FindFirstOperation<ILockOperation>(x),
            static (walker, operation) => walker.VisitLock(operation, new()),
            "Lock statements are not supported");

        AssertUnsupportedByDispatch(code, OperationKind.Lock, "Lock statements are not supported");
    }

    [TestMethod]
    public void VisitDynamicObjectCreation_NotSupported()
    {
        const string code = """
            class TestClass
            {
                void TestMethod(dynamic value)
                {
                    var builder = new System.Text.StringBuilder(value);
                }
            }
            """;

        var block = GetBlockOperation(code);
        AssertUnsupportedDirect(
            block,
            static x => FindFirstOperation<IDynamicObjectCreationOperation>(x),
            static (walker, operation) => walker.VisitDynamicObjectCreation(operation, new()),
            "Dynamic object creation is not supported");

        AssertUnsupportedByDispatch(code, OperationKind.DynamicObjectCreation, "Dynamic object creation is not supported");
    }

    [TestMethod]
    public void VisitDynamicMemberReference_NotSupported()
    {
        const string code = """
            class TestClass
            {
                void TestMethod(dynamic source)
                {
                    var value = source.Name;
                }
            }
            """;

        var block = GetBlockOperation(code);
        AssertUnsupportedDirect(
            block,
            static x => FindFirstOperation<IDynamicMemberReferenceOperation>(x),
            static (walker, operation) => walker.VisitDynamicMemberReference(operation, new()),
            "Dynamic member references are not supported");

        AssertUnsupportedByDispatch(code, OperationKind.DynamicMemberReference, "Dynamic member references are not supported");
    }

    [TestMethod]
    public void VisitDynamicInvocation_NotSupported()
    {
        const string code = """
            class TestClass
            {
                void TestMethod(dynamic source)
                {
                    source.Run(1);
                }
            }
            """;

        var block = GetBlockOperation(code);
        AssertUnsupportedDirect(
            block,
            static x => FindFirstOperation<IDynamicInvocationOperation>(x),
            static (walker, operation) => walker.VisitDynamicInvocation(operation, new()),
            "Dynamic method invocations are not supported");

        AssertUnsupportedByDispatch(code, OperationKind.DynamicInvocation, "Dynamic method invocations are not supported");
    }

    [TestMethod]
    public void VisitDynamicIndexerAccess_NotSupported()
    {
        const string code = """
            class TestClass
            {
                void TestMethod(dynamic source)
                {
                    var value = source["name"];
                }
            }
            """;

        var block = GetBlockOperation(code);
        AssertUnsupportedDirect(
            block,
            static x => FindFirstOperation<IDynamicIndexerAccessOperation>(x),
            static (walker, operation) => walker.VisitDynamicIndexerAccess(operation, new()),
            "Dynamic indexer access is not supported");

        AssertUnsupportedByDispatch(code, OperationKind.DynamicIndexerAccess, "Dynamic indexer access is not supported");
    }

    [TestMethod]
    public void VisitTranslatedQuery_NotSupported()
    {
        const string code = """
            class TestClass
            {
                void TestMethod(int[] source)
                {
                    var query = from item in source where item > 0 select item;
                }
            }
            """;

        var block = GetBlockOperation(code);
        AssertUnsupportedDirect(
            block,
            static x => FindFirstOperation<ITranslatedQueryOperation>(x),
            static (walker, operation) => walker.VisitTranslatedQuery(operation, new()),
            "Translated LINQ queries are not supported");

        AssertUnsupportedByDispatch(code, OperationKind.TranslatedQuery, "Translated LINQ queries are not supported");
    }

    [TestMethod]
    public void VisitTypeOf_NotSupported()
    {
        const string code = """
            class TestClass
            {
                void TestMethod()
                {
                    var type = typeof(int);
                }
            }
            """;

        var block = GetBlockOperation(code);
        AssertUnsupportedDirect(
            block,
            static x => FindFirstOperation<ITypeOfOperation>(x),
            static (walker, operation) => walker.VisitTypeOf(operation, new()),
            "typeof operator is not supported");

        AssertUnsupportedByDispatch(code, OperationKind.TypeOf, "typeof operator is not supported");
    }

    [TestMethod]
    public void VisitRangeOperation_NotSupported()
    {
        const string code = """
            class TestClass
            {
                void TestMethod()
                {
                    System.Range range = 1..3;
                }
            }
            """;

        var block = GetBlockOperation(code);
        AssertUnsupportedDirect(
            block,
            static x => FindFirstOperation<IRangeOperation>(x),
            static (walker, operation) => walker.VisitRangeOperation(operation, new()),
            "Standalone range operations are not supported");

        AssertUnsupportedByDispatch(code, OperationKind.Range, "Standalone range operations are not supported");
    }

    [TestMethod]
    public void VisitUsingStatementForm_NotSupported()
    {
        const string code = """
            class DisposableThing : System.IDisposable
            {
                public void Dispose()
                {
                }
            }

            class TestClass
            {
                void TestMethod()
                {
                    using (new DisposableThing())
                    {
                        Console.WriteLine(1);
                    }
                }
            }
            """;

        var block = GetBlockOperation(code);
        AssertUnsupportedDirect(
            block,
            static x => FindFirstOperation<IUsingOperation>(x),
            static (walker, operation) => walker.VisitUsing(operation, new()),
            "Using statements are not supported");

        AssertUnsupportedByDispatch(code, OperationKind.Using, "Using statements are not supported");
    }

    [TestMethod]
    public void Visit_WhenCancellationRequested_ThrowsOperationCanceledException()
    {
        const string code = """
            class TestClass
            {
                void TestMethod()
                {
                    int value = 1;
                }
            }
            """;

        var block = GetBlockOperation(code);
        using var cts = new System.Threading.CancellationTokenSource();
        cts.Cancel();

        var walker = new SemanticWalker(true, cts.Token);
        Assert.Throws<OperationCanceledException>(() => walker.Visit(block, new()));
    }

    [TestMethod]
    public void Visit_WhenReportIsNull_AttachesLocationMetadataToTransformationException()
    {
        const string code = """
            class TestClass
            {
                void TestMethod()
                {
                    object gate = new object();
                    lock (gate)
                    {
                        Console.WriteLine(gate);
                    }
                }
            }
            """;

        var block = GetBlockOperation(code);
        var walker = new SemanticWalker(true);
        var exception = Assert.Throws<OperationTransformationException>(() => walker.Visit(block, new()));
        Assert.AreEqual(OperationKind.Lock, exception.Kind);

        var path = exception.Data["location.path"] as string;
        Assert.IsFalse(string.IsNullOrWhiteSpace(path));

        var startLineRaw = exception.Data["location.startLine"];
        Assert.IsTrue(startLineRaw is int);
        var startLineValue = (int)startLineRaw!;
        Assert.IsTrue(startLineValue >= 1);

        Assert.IsTrue(exception.Data["location.startColumn"] is int startColumn && startColumn >= 1);

        var endLineRaw = exception.Data["location.endLine"];
        Assert.IsTrue(endLineRaw is int);
        var endLineValue = (int)endLineRaw!;
        Assert.IsTrue(endLineValue >= startLineValue);

        Assert.IsTrue(exception.Data["location.endColumn"] is int endColumn && endColumn >= 1);
    }

    [TestMethod]
    public void ConvertFromSyntaxNode_WhenReportIsNull_AttachesLocationMetadataToSyntaxTransformationException()
    {
        var syntax = SyntaxFactory.ParseStatement("int value = 1;");
        var method = typeof(SemanticWalker).GetMethod(
            "ConvertFromSyntaxNode",
            BindingFlags.Instance | BindingFlags.NonPublic,
            binder: null,
            types: [typeof(SyntaxNode)],
            modifiers: null);
        Assert.IsNotNull(method, "未找到 ConvertFromSyntaxNode 私有方法。");

        var walker = new SemanticWalker(true);
        try
        {
            method.Invoke(walker, [syntax]);
            Assert.Fail("Expected SyntaxNodeTransformationException but no exception was thrown.");
        }
        catch (TargetInvocationException invocation) when (invocation.InnerException is SyntaxNodeTransformationException exception)
        {
            Assert.AreEqual(syntax.Kind(), exception.Kind);
            StringAssert.Contains(exception.Message ?? string.Empty, "Unsupported syntax node kind");

            var path = exception.Data["location.path"] as string;
            Assert.IsFalse(string.IsNullOrWhiteSpace(path));

            var startLineRaw = exception.Data["location.startLine"];
            Assert.IsTrue(startLineRaw is int);
            var startLineValue = (int)startLineRaw!;
            Assert.IsTrue(startLineValue >= 1);

            Assert.IsTrue(exception.Data["location.startColumn"] is int startColumn && startColumn >= 1);

            var endLineRaw = exception.Data["location.endLine"];
            Assert.IsTrue(endLineRaw is int);
            var endLineValue = (int)endLineRaw!;
            Assert.IsTrue(endLineValue >= startLineValue);

            Assert.IsTrue(exception.Data["location.endColumn"] is int endColumn && endColumn >= 1);
        }
    }

    [TestMethod]
    public void VisitNotSupportHandlers_AllRejectWithTransformationException()
    {
        var handlers = LoadNotSupportHandlers();
        Assert.IsTrue(handlers.Count >= 30, $"NotSupport 处理器数量异常，实际为 {handlers.Count}。");

        var walker = new SemanticWalker(true);
        foreach (var handler in handlers)
        {
            var method = typeof(SemanticWalker).GetMethod(
                handler.MethodName,
                BindingFlags.Instance | BindingFlags.Public,
                binder: null,
                types: [handler.OperationType, typeof(SenseArgument)],
                modifiers: null);
            Assert.IsNotNull(method, $"未找到处理器方法: {handler.MethodName}({handler.OperationType.Name})");

            var operation = CreateOperationProxy(handler.OperationType, OperationKind.None);
            try
            {
                method.Invoke(walker, [operation, new SenseArgument()]);
                Assert.Fail($"处理器未拒绝: {handler.MethodName}({handler.OperationType.Name})");
            }
            catch (TargetInvocationException invocation) when (invocation.InnerException is OperationTransformationException exception)
            {
                Assert.AreEqual(OperationKind.None, exception.Kind, $"处理器未透传 operation.Kind: {handler.MethodName}");
                var message = (exception.Message ?? string.Empty).ToLowerInvariant();
                StringAssert.Contains(message, "not supported");
            }
        }
    }

    [TestMethod]
    public void SemanticWalker_CoversCurrentRoslynOperationVisitorSurface()
    {
        var semanticWalkerMethods = typeof(SemanticWalker)
            .GetMethods(BindingFlags.Instance | BindingFlags.Public)
            .Where(static method => method.Name.StartsWith("Visit", StringComparison.Ordinal))
            .Where(static method =>
            {
                var parameters = method.GetParameters();
                return parameters.Length == 2 &&
                    typeof(IOperation).IsAssignableFrom(parameters[0].ParameterType) &&
                    parameters[1].ParameterType == typeof(SenseArgument);
            })
            .Select(static method => (method.Name, OperationType: method.GetParameters()[0].ParameterType))
            .ToHashSet();

        var visitorMethods = typeof(OperationVisitor<SenseArgument, EsNode?>)
            .GetMethods(BindingFlags.Instance | BindingFlags.Public)
            .Where(static method => method.Name.StartsWith("Visit", StringComparison.Ordinal))
            .Where(static method =>
            {
                var parameters = method.GetParameters();
                return parameters.Length == 2 &&
                    typeof(IOperation).IsAssignableFrom(parameters[0].ParameterType) &&
                    parameters[1].ParameterType == typeof(SenseArgument);
            })
            .Where(static method => ShouldHaveExplicitSemanticWalkerCoverage(method.GetParameters()[0].ParameterType))
            .Select(static method => (method.Name, OperationType: method.GetParameters()[0].ParameterType))
            .ToArray();

        var missing = visitorMethods
            .Where(method => !semanticWalkerMethods.Contains(method))
            .Select(static method => $"{method.Name}({method.OperationType.Name})")
            .Order(StringComparer.Ordinal)
            .ToArray();

        Assert.IsTrue(
            missing.Length == 0,
            "SemanticWalker must explicitly support or reject every concrete Roslyn IOperation visitor method. Missing: " + string.Join(", ", missing));
    }

    private static bool ShouldHaveExplicitSemanticWalkerCoverage(Type operationType)
    {
        if (operationType == typeof(IOperation))
            return false;

        return operationType.Name is not (
            nameof(IAssignmentOperation) or
            "IAggregateQueryOperation" or
            nameof(IInterpolatedStringContentOperation) or
            nameof(ILoopOperation) or
            nameof(IMemberReferenceOperation) or
            nameof(IMethodBodyBaseOperation) or
            nameof(IPatternOperation) or
            "IPlaceholderOperation" or
            nameof(ISymbolInitializerOperation));
    }
}
