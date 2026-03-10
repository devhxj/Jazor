using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Jazor.ComplierTest;

[TestClass]
public class OutputGenTest
{
    [TestMethod]
    public async Task Generate_All_Outputs()
    {
        var tests = new Dictionary<string, string>
        {
            ["Convert_SimplePublicClass_ReturnsModule"] = """
                public static class TestClass
                {
                    public static int Field = 42;
                    public static void Method() { }
                }
                """,

            ["Convert_ClassWithStaticField_GeneratesVariableDeclaration"] = """
                public static class TestClass
                {
                    public static int Field = 42;
                }
                """,

            ["Convert_ClassWithConstField_GeneratesConstDeclaration"] = """
                public static class TestClass
                {
                    public const int ConstField = 42;
                }
                """,

            ["Convert_ClassWithPrivateField_DoesNotExport"] = """
                public static class TestClass
                {
                    private static int PrivateField = 42;
                    public static int PublicField = 24;
                }
                """,

            ["Convert_ClassWithMethod_GeneratesFunctionDeclaration"] = """
                public static class TestClass
                {
                    public static int TestMethod()
                    {
                        return 1;
                    }
                }
                """,

            ["Convert_ClassWithProperty_GeneratesPropertyMethods"] = """
                public static class TestClass
                {
                    public static int Property { get; set; }
                }
                """,

            ["Convert_ClassWithNestedClass_GeneratesClassDeclaration"] = """
                public static class TestClass
                {
                    public static class NestedClass
                    {
                        public static int Field = 42;
                    }
                }
                """,

            ["Convert_PrivateNestedClass_NoExport"] = """
                public static class TestClass
                {
                    private static class NestedClass
                    {
                        public static int Field = 42;
                    }
                }
                """,

            ["Convert_ClassWithEnum_GeneratesConstObject"] = """
                public static class TestClass
                {
                    public enum Status
                    {
                        Active,
                        Inactive,
                        Pending = 10
                    }
                }
                """,

            ["Convert_PrivateEnum_NoExport"] = """
                public static class TestClass
                {
                    private enum InternalEnum
                    {
                        A,
                        B
                    }
                }
                """,

            ["Convert_ClassWithStaticConstructor_GeneratesInit"] = """
                public static class TestClass
                {
                    public static int Value;
                    static TestClass()
                    {
                        Value = 100;
                    }
                }
                """,

            ["Convert_MethodWithParameters_GeneratesFunctionWithParams"] = """
                public static class TestClass
                {
                    public static int Add(int a, int b) => a + b;
                }
                """,

            ["Convert_MethodWithBody_GeneratesFunctionWithBody"] = """
                public static class TestClass
                {
                    public static int Calculate(int x)
                    {
                        var result = x * 2;
                        return result;
                    }
                }
                """,

            ["Convert_MethodOverloads_GeneratesUniqueNames"] = """
                public static class TestClass
                {
                    public static void Method() { }
                    public static void Method(int a) { }
                    public static int Method(int a, int b) => a + b;
                }
                """,

            ["Convert_ReadOnlyProperty_GeneratesOnlyGetter"] = """
                public static class TestClass
                {
                    public static int Value { get; }
                }
                """,

            ["Convert_PropertyWithExpressionBody_GeneratesGetter"] = """
                public static class TestClass
                {
                    public static int Value => 42;
                }
                """,

            ["Convert_PrivateMethod_DoesNotExport"] = """
                public static class TestClass
                {
                    private static void PrivateMethod() { }
                    public static void PublicMethod() { }
                }
                """,

            ["Convert_PrivateProperty_DoesNotExport"] = """
                public static class TestClass
                {
                    private static int PrivateProp { get; set; }
                    public static int PublicProp { get; set; }
                }
                """,

            ["Convert_StringField_GeneratesStringLiteral"] = """
                public static class TestClass
                {
                    public static string Message = "Hello";
                }
                """,

            ["Convert_InterpolatedString_GeneratesTemplateLiteral"] = """
                public static class TestClass
                {
                    public static string Greet(string name) => $"Hello, {name}!";
                }
                """,
        };

        Console.WriteLine("// ========== 测试期望输出 ==========");
        Console.WriteLine($"// 共 {tests.Count} 个测试");
        Console.WriteLine();

        foreach (var (name, code) in tests)
        {
            var output = await TestHelper.GetOutputAsync(code);
            Console.WriteLine($"// ===== {name} =====");
            if (output != null)
            {
                Console.WriteLine("// 期望输出:");
                Console.WriteLine(output);
            }
            else
            {
                Console.WriteLine("// 无法获取输出（可能抛出异常）");
            }
            Console.WriteLine();
        }
    }
}
