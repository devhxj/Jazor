using Acornima.Ast;
using Jazor.Compiler;
using Jazor.ComplierTest;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Operations;
using System.Reflection;

namespace Jazor.CompilerTest;

[TestClass]
public sealed class SemanticWalkerTranslationGuardContractTests
{
    [TestMethod]
    public void TranslateGuards_PreserveStrictOptionalAndCollectionFailureContracts()
    {
        var literal = GetLiteralOperation();
        var walker = new SemanticWalker(true);
        var strict = GetTranslateDefinition(parameterCount: 2);
        var optional = GetTranslateDefinition(parameterCount: 3, firstParameterIsCollection: false);
        var collection = GetTranslateDefinition(parameterCount: 3, firstParameterIsCollection: true);
        var nullableCollection = GetTranslateDefinition(parameterCount: 4);

        var translated = strict.MakeGenericMethod(typeof(Expression)).Invoke(
            walker,
            [literal, new SenseArgument()]);
        Assert.IsInstanceOfType<Expression>(translated);

        var strictFailure = Assert.Throws<TargetInvocationException>(() =>
            strict.MakeGenericMethod(typeof(Statement)).Invoke(walker, [literal, new SenseArgument()]));
        Assert.IsInstanceOfType<OperationTransformationException>(strictFailure.InnerException);

        var fallback = new Identifier("fallback");
        Assert.AreSame(
            fallback,
            optional.MakeGenericMethod(typeof(Expression)).Invoke(
                walker,
                [null, new SenseArgument(), fallback]));
        Assert.IsNull(optional.MakeGenericMethod(typeof(Statement)).Invoke(
            walker,
            [literal, new SenseArgument(), null]));

        var expressions = new List<Expression>();
        collection.MakeGenericMethod(typeof(Expression)).Invoke(walker, [expressions, null, new SenseArgument()]);
        Assert.IsEmpty(expressions);
        collection.MakeGenericMethod(typeof(Expression)).Invoke(walker, [expressions, literal, new SenseArgument()]);
        Assert.HasCount(1, expressions);
        var statements = new List<Statement>();
        collection.MakeGenericMethod(typeof(Statement)).Invoke(walker, [statements, literal, new SenseArgument()]);
        Assert.IsEmpty(statements);

        var nullableExpressions = new List<Expression?>();
        nullableCollection.MakeGenericMethod(typeof(Expression)).Invoke(
            walker,
            [nullableExpressions, null, new SenseArgument(), null]);
        Assert.IsEmpty(nullableExpressions);
        nullableCollection.MakeGenericMethod(typeof(Expression)).Invoke(
            walker,
            [nullableExpressions, literal, new SenseArgument(), null]);
        Assert.HasCount(1, nullableExpressions);
        var nullableStatements = new List<Statement?>();
        nullableCollection.MakeGenericMethod(typeof(Statement)).Invoke(
            walker,
            [nullableStatements, literal, new SenseArgument(), null]);
        Assert.IsEmpty(nullableStatements);
    }

    private static MethodInfo GetTranslateDefinition(int parameterCount, bool? firstParameterIsCollection = null)
        => typeof(SemanticWalker)
            .GetMethods(BindingFlags.Instance | BindingFlags.NonPublic)
            .Single(method =>
            {
                if (method.Name != "Translate" ||
                    !method.IsGenericMethodDefinition ||
                    method.GetParameters().Length != parameterCount)
                {
                    return false;
                }

                if (firstParameterIsCollection is null)
                    return true;

                var firstParameter = method.GetParameters()[0].ParameterType;
                var isCollection = firstParameter.IsGenericType &&
                    firstParameter.GetGenericTypeDefinition() == typeof(ICollection<>);
                return isCollection == firstParameterIsCollection.Value;
            });

    private static ILiteralOperation GetLiteralOperation()
    {
        const string source = """
            public sealed class TestClass
            {
                void TestMethod()
                {
                    var value = 42;
                }
            }
            """;
        var syntaxTree = CSharpSyntaxTree.ParseText(source, TestMetadataReferences.PreviewParseOptions);
        var compilation = CSharpCompilation.Create(
            "SemanticWalkerTranslationGuards",
            [syntaxTree],
            TestMetadataReferences.Net11,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
        var errors = compilation.GetDiagnostics()
            .Where(static diagnostic => diagnostic.Severity == DiagnosticSeverity.Error)
            .ToArray();
        Assert.IsEmpty(errors, string.Join(Environment.NewLine, errors.Select(static error => error.ToString())));

        var method = syntaxTree.GetRoot()
            .DescendantNodes()
            .OfType<MethodDeclarationSyntax>()
            .Single(static declaration => declaration.Identifier.ValueText == "TestMethod");
        var block = Assert.IsInstanceOfType<IBlockOperation>(compilation.GetSemanticModel(syntaxTree).GetOperation(method.Body!));
        return block.DescendantsAndSelf().OfType<ILiteralOperation>()
            .Single(static operation => operation.ConstantValue.Value is 42);
    }
}
