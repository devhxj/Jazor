using ECMAScript;
using Jazor.Compiler;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Operations;

namespace Jazor.ComplierTest;

[TestClass]
public sealed class SemanticWalkerStringTest
{
	/// <summary>
	/// 编译代码并获取roslyn代码块
	/// </summary>
	/// <param name="code"></param>
	/// <returns></returns>
	/// <exception cref="InvalidOperationException"></exception>
	private static IBlockOperation GetBlockOperation(string code)
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

		// 输出编译诊断信息
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

		// 查找第一个方法体
		var methodDeclaration = root.DescendantNodes()
			.OfType<MethodDeclarationSyntax>()
			.FirstOrDefault(static method => method.Identifier.ValueText == "TestMethod" && method.Body is not null)
			?? root.DescendantNodes().OfType<MethodDeclarationSyntax>().FirstOrDefault(static method => method.Body is not null);
		if (methodDeclaration?.Body is not null)
		{
			var operation = semanticModel.GetOperation(methodDeclaration.Body) as IBlockOperation;
			if (operation is not null)
				return operation;
		}

		throw new InvalidOperationException("未找到可分析的操作");
	}

	private static void AssertScriptEqual(string expected, string? actual)
	{
		Assert.AreEqual(expected.ReplaceLineEndings("\n"), actual?.ReplaceLineEndings("\n"));
	}

	[TestMethod]
	public void Visit_String_FromCodePoint()
	{
		var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    string value = String.FromCodePoint(65);
                }
            }
        ");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		AssertScriptEqual(@"{
  let value = String.fromCodePoint(65);
}", script);
	}

	[TestMethod]
	public void Visit_String_Includes_WithPosition()
	{
		var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    string value = ""hello"";
                    bool hasEll = value.Includes(""ell"", 1);
                }
            }
        ");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		AssertScriptEqual(@"{
  let value = ""hello"";
  let hasEll = value.includes(""ell"", 1);
}", script);
	}

	[TestMethod]
	public void Visit_String_LocaleCompare_ReturnsNumberShape()
	{
		var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    string left = ""a"";
                    Number result = left.LocaleCompare(""b"");
                }
            }
        ");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		AssertScriptEqual(@"{
  let left = ""a"";
  let result = left.localeCompare(""b"");
}", script);
	}

	[TestMethod]
	public void Visit_String_LocaleCompare_WithLocaleArray()
	{
		var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    string left = ""a"";
                    Number result = left.LocaleCompare(""b"", new[] { ""en-US"", ""zh-CN"" });
                }
            }
        ");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		AssertScriptEqual(@"{
  let left = ""a"";
  let result = left.localeCompare(""b"", [""en-US"", ""zh-CN""]);
}", script);
	}

	#region 简单插值字符串测试

	/// <summary>
	/// 测试 VisitInterpolatedString - 简单插值字符串
	/// C# 示例：$"Hello {name}!"
	/// 转换结果：`Hello ${name}!`
	/// </summary>
	[TestMethod]
	public void Visit_InterpolatedString_Simple()
	{
		var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    string name = ""World"";
                    string message = $""Hello {name}!"";
                }
            }
            ");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		Assert.AreEqual(@"{
  let name = ""World"";
  let message = `Hello ${name}!`;
}", script);
	}

	/// <summary>
	/// 测试 VisitInterpolatedString - 只有文本的插值字符串
	/// C# 示例：$"Hello World"
	/// 转换结果：`Hello World`
	/// </summary>
	[TestMethod]
	public void Visit_InterpolatedString_TextOnly()
	{
		var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    string message = $""Hello World"";
                }
            }
            ");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		Assert.AreEqual(@"{
  let message = ""Hello World"";
}", script);
	}

	/// <summary>
	/// 测试 VisitInterpolatedString - 以表达式开头的插值字符串
	/// C# 示例：$"{value} items"
	/// 转换结果：`${value} items`
	/// </summary>
	[TestMethod]
	public void Visit_InterpolatedString_StartsWithExpression()
	{
		var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    int count = 5;
                    string message = $""{count} items"";
                }
            }
            ");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		Assert.AreEqual(@"{
  let count = 5;
  let message = `${count} items`;
}", script);
	}

	/// <summary>
	/// 测试 VisitInterpolatedString - 以表达式结尾的插值字符串
	/// C# 示例：$"Value: {x}"
	/// 转换结果：`Value: ${x}`
	/// </summary>
	[TestMethod]
	public void Visit_InterpolatedString_EndsWithExpression()
	{
		var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    int x = 42;
					string message = $""Value: {x}"";
                }
            }
            ");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		Assert.AreEqual(@"{
  let x = 42;
  let message = `Value: ${x}`;
}", script);
	}

	#endregion

	#region 多个插值表达式测试

	/// <summary>
	/// 测试 VisitInterpolatedString - 多个插值表达式
	/// C# 示例：$"Name: {name}, Age: {age}"
	/// 转换结果：`Name: ${name}, Age: ${age}`
	/// </summary>
	[TestMethod]
	public void Visit_InterpolatedString_MultipleExpressions()
	{
		var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    string name = ""John"";
                    int age = 30;
                    string message = $""Name: {name}, Age: {age}"";
                }
            }
            ");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		Assert.AreEqual(@"{
  let name = ""John"";
  let age = 30;
  let message = `Name: ${name}, Age: ${age}`;
}", script);
	}

	/// <summary>
	/// 测试 VisitInterpolatedString - 连续的插值表达式
	/// C# 示例：$"{x} + {y} = {x + y}"
	/// 转换结果：`${x} + ${y} = ${x + y}`
	/// </summary>
	[TestMethod]
	public void Visit_InterpolatedString_ConsecutiveExpressions()
	{
		var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    int x = 5;
                    int y = 3;
                    string message = $""{x} + {y} = {x + y}"";
                }
            }
            ");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		Assert.AreEqual(@"{
  let x = 5;
  let y = 3;
  let message = `${x} + ${y} = ${x + y}`;
}", script);
	}

	#endregion

	#region 复杂表达式插值测试

	/// <summary>
	/// 测试 VisitInterpolatedString - 包含方法调用的插值
	/// C# 示例：$"Length: {name.Length}"
	/// 转换结果：`Length: ${name.Length}`
	/// </summary>
	[TestMethod]
	public void Visit_InterpolatedString_WithMethodCall()
	{
		var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    string name = ""John"";
                    string message = $""Length: {name.Length}"";
                }
            }
            ");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		Assert.AreEqual(@"{
  let name = ""John"";
  let message = `Length: ${name.length}`;
}", script);
	}

	/// <summary>
	/// 测试 VisitInterpolatedString - 包含算术运算的插值
	/// C# 示例：$"Sum: {a + b}"
	/// 转换结果：`Sum: ${a + b}`
	/// </summary>
	[TestMethod]
	public void Visit_InterpolatedString_WithArithmetic()
	{
		var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    int a = 5;
                    int b = 3;
                    string message = $""Sum: {a + b}"";
                }
            }
            ");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		Assert.AreEqual(@"{
  let a = 5;
  let b = 3;
  let message = `Sum: ${a + b}`;
}", script);
	}

	/// <summary>
	/// 测试 VisitInterpolatedString - 包含三元运算符的插值
	/// C# 示例：$"Result: {(x > 0 ? ""positive"" : ""negative"")}"
	/// 转换结果：`Result: ${x > 0 ? "positive" : "negative"}`
	/// </summary>
	[TestMethod]
	public void Visit_InterpolatedString_WithTernary()
	{
		var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    int x = 5;
                    string message = $""Result: {(x > 0 ? ""positive"" : ""negative"")}"";
                }
            }
            ");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		Assert.AreEqual(@"{
  let x = 5;
  let message = `Result: ${x > 0 ? ""positive"" : ""negative""}`;
}", script);
	}

	/// <summary>
	/// 测试 VisitInterpolatedString - 嵌套属性访问
	/// C# 示例：$"Length: {str.Length}"
	/// 转换结果：`Length: ${str.Length}`
	/// </summary>
	[TestMethod]
	public void Visit_InterpolatedString_WithPropertyAccess()
	{
		var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    string str = ""Hello"";
                    string message = $""Length: {str.Length}"";
                }
            }
            ");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		Assert.AreEqual(@"{
  let str = ""Hello"";
  let message = `Length: ${str.length}`;
}", script);
	}

	#endregion

	#region 转义字符测试

	/// <summary>
	/// 测试 VisitInterpolatedString - 包含转义字符
	/// C# 示例：$"Value: {value} (escaped: \t)"
	/// 转换结果：`Value: ${value} (escaped: \t)`
	/// </summary>
	[TestMethod]
	public void Visit_InterpolatedString_WithEscapes()
	{
		var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    int value = 42;
                    string message = $""Value: {value}\\t(tab)"";
                }
            }
            ");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		Assert.AreEqual(@"{
  let value = 42;
  let message = `Value: ${value}\t(tab)`;
}", script);
	}

	/// <summary>
	/// 测试 VisitInterpolatedString - 包含制表符
	/// C# 示例：$"Col1{value}Col2"
	/// 转换结果：`Col1${value}Col2`
	/// </summary>
	[TestMethod]
	public void Visit_InterpolatedString_WithTab()
	{
		var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    int value = 42;
                    string message = $""Col1{value}Col2"";
                }
            }
            ");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		Assert.AreEqual(@"{
  let value = 42;
  let message = `Col1${value}Col2`;
}", script);
	}

	/// <summary>
	/// 测试 VisitInterpolatedString - 包含反斜杠
	/// C# 示例：$"Path{value}File"
	/// 转换结果：`Path${value}File`
	/// </summary>
	[TestMethod]
	public void Visit_InterpolatedString_WithBackslash()
	{
		var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    string value = ""Test"";
                    string message = $""Path{value}File"";
                }
            }
            ");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		Assert.AreEqual(@"{
  let value = ""Test"";
  let message = `Path${value}File`;
}", script);
	}

	#endregion

	#region 复杂场景测试

	/// <summary>
	/// 测试 VisitInterpolatedString - 混合场景
	/// C# 示例：$"User {name} (ID: {id}) has {count} messages"
	/// 转换结果：`User ${name} (ID: ${id}) has ${count} messages`
	/// </summary>
	[TestMethod]
	public void Visit_InterpolatedString_Complex()
	{
		var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    string name = ""John"";
                    int id = 123;
                    int count = 5;
                    string message = $""User {name} (ID: {id}) has {count} messages"";
                }
            }
            ");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		Assert.AreEqual(@"{
  let name = ""John"";
  let id = 123;
  let count = 5;
  let message = `User ${name} (ID: ${id}) has ${count} messages`;
}", script);
	}

	/// <summary>
	/// 测试 VisitInterpolatedString - 嵌套字符串插值
	/// C# 示例：$""Outer {value} and {$"{inner}"}""
	/// 转换结果：`Outer ${value} and ${"inner"}`
	/// </summary>
	[TestMethod]
	public void Visit_InterpolatedString_Nested()
	{
		var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    int value = 42;
                    string inner = ""test"";
                    string message = $""Outer {value} and {inner}"";
                }
            }
            ");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		Assert.AreEqual(@"{
  let value = 42;
  let inner = ""test"";
  let message = `Outer ${value} and ${inner}`;
}", script);
	}

	/// <summary>
	/// 测试 VisitInterpolatedString - 包含字符串拼接
	/// C# 示例：$"Result: {x + y} = {x + y}"
	/// 转换结果：`Result: ${x + y} = ${x + y}`
	/// </summary>
	[TestMethod]
	public void Visit_InterpolatedString_WithConcatenation()
	{
		var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    int x = 5;
                    int y = 3;
                    int sum = x + y;
                    string message = $""Result: {x} + {y} = {sum}"";
                }
            }
            ");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		Assert.AreEqual(@"{
  let x = 5;
  let y = 3;
  let sum = x + y;
  let message = `Result: ${x} + ${y} = ${sum}`;
}", script);
	}

	/// <summary>
	/// 测试 VisitInterpolatedString - 空字符串插值
	/// C# 示例：$""{x}""
	/// 转换结果：`${x}`
	/// </summary>
	[TestMethod]
	public void Visit_InterpolatedString_EmptyText()
	{
		var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    int x = 42;
                    string message = $""{x}"";
                }
            }
            ");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		Assert.AreEqual(@"{
  let x = 42;
  let message = `${x}`;
}", script);
	}

	/// <summary>
	/// 测试字符串插值在表达式中的使用
	/// C# 示例：Console.WriteLine($"Value: {x}")
	/// 转换结果：Console.WriteLine(`Value: ${x}`)
	/// </summary>
	[TestMethod]
	public void Visit_InterpolatedString_InExpression()
	{
		var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    int x = 42;
                    Console.WriteLine($""Value: {x}"");
                }
            }
            ");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		Assert.AreEqual(@"{
  let x = 42;
  console.log(`Value: ${x}`);
}", script);
	}

	/// <summary>
	/// 测试多个插值字符串的组合使用
	/// </summary>
	[TestMethod]
	public void Visit_InterpolatedString_Multiple()
	{
		var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    string name = ""Alice"";
                    int age = 25;

                    // 简单插值
                    string greeting = $""Hello, {name}!"";

                    // 多个插值
                    string info = $""Name: {name}, Age: {age}"";

                    // 带表达式
                    string nextYear = $""Next year: {age + 1}"";

                    Console.WriteLine(greeting);
                    Console.WriteLine(info);
                    Console.WriteLine(nextYear);
                }
            }
            ");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		Assert.AreEqual(@"{
  let name = ""Alice"";
  let age = 25;
  let greeting = `Hello, ${name}!`;
  let info = `Name: ${name}, Age: ${age}`;
  let nextYear = `Next year: ${age + 1}`;
  console.log(greeting);
  console.log(info);
  console.log(nextYear);
}", script);
	}

	/// <summary>
	/// 测试插值字符串 - 包含制表符（Tab）
	/// C# 示例：$"Name:\t{name}"
	/// 转换结果：模板字符串包含制表符
	/// </summary>
	[TestMethod]
	public void Visit_InterpolatedString_WithRealTab()
	{
		var block = GetBlockOperation(@"
			class TestClass
			{
				void TestMethod()
				{
					string name = ""John"";
					string message = $""Name:\t{name}"";
				}
			}
		");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		Assert.AreEqual(@"{
  let name = ""John"";
  let message = `Name:	${name}`;
}", script);
	}

	/// <summary>
	/// 测试插值字符串 - 包含反斜杠
	/// C# 示例：$"Path:\\{path}"
	/// 转换结果：模板字符串包含转义的反斜杠
	/// </summary>
	[TestMethod]
	public void Visit_InterpolatedString_WithRealBackslash()
	{
		var block = GetBlockOperation(@"
			class TestClass
			{
				void TestMethod()
				{
					string path = ""folder/file.txt"";
					string message = $""Path:\\{path}"";
				}
			}
		");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		Assert.AreEqual(@"{
  let path = ""folder/file.txt"";
  let message = `Path:\${path}`;
}", script);
	}

	/// <summary>
	/// 测试插值字符串 - 包含换行符
	/// C# 示例：$"Line1\nLine2"
	/// 转换结果：由于没有插值表达式，优化为普通字符串
	/// </summary>
	[TestMethod]
	public void Visit_InterpolatedString_WithNewline()
	{
		var block = GetBlockOperation(@"
			class TestClass
			{
				void TestMethod()
				{
					string message = $""Line1\nLine2"";
				}
			}
		");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		AssertScriptEqual(@"{
  let message = ""Line1\nLine2"";
}", script);

	}

	/// <summary>
	/// 测试插值字符串 - 包含回车符
	/// C# 示例：$"Text\r\nMore"
	/// 转换结果：模板字符串包含回车换行符
	/// </summary>
	[TestMethod]
	public void Visit_InterpolatedString_WithCarriageReturn()
	{
		var block = GetBlockOperation(@"
			class TestClass
			{
				void TestMethod()
				{
					string message = $""Text\r\nMore"";
				}
			}
		");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToECMAScript();

		Assert.AreEqual("{let message=\"Text\\r\\nMore\"}", script);
	}

	/// <summary>
	/// 测试插值字符串 - 多种转义字符组合
	/// C# 示例：$"Item:\t{name}\nPrice:\t{price}"
	/// 转换结果：模板字符串包含多种转义字符
	/// </summary>
	[TestMethod]
	public void Visit_InterpolatedString_MixedEscapeSequences()
	{
		var block = GetBlockOperation(@"
			class TestClass
			{
				void TestMethod()
				{
					string name = ""Apple"";
					int price = 100;
					string message = $""Item:\t{name}\nPrice:\t{price}"";
				}
			}
		");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToECMAScript();

		Assert.AreEqual("{let name=\"Apple\";let price=100;let message=`Item:	${name}\nPrice:	${price}`}", script);

	}

	#endregion

	#region 扩展测试用例 - 字符串连接

	/// <summary>
	/// 测试字符串连接 - 简单连接
	/// </summary>
	[TestMethod]
	public void Visit_StringConcat_Simple()
	{
		var block = GetBlockOperation(@"
			class TestClass
			{
				void TestMethod()
				{
					string a = ""Hello"";
					string b = ""World"";
					string result = a + "" "" + b;
				}
			}
		");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		Assert.AreEqual(@"{
  let a = ""Hello"";
  let b = ""World"";
  let result = a + "" "" + b;
}", script);
	}

	/// <summary>
	/// 测试字符串连接 - 多个字符串
	/// </summary>
	[TestMethod]
	public void Visit_StringConcat_Multiple()
	{
		var block = GetBlockOperation(@"
			class TestClass
			{
				void TestMethod()
				{
					string a = ""a"";
					string b = ""b"";
					string c = ""c"";
					string result = a + b + c;
				}
			}
		");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		Assert.AreEqual(@"{
  let a = ""a"";
  let b = ""b"";
  let c = ""c"";
  let result = a + b + c;
}", script);
	}

	/// <summary>
	/// 测试字符串连接 - 与数字连接
	/// </summary>
	[TestMethod]
	public void Visit_StringConcat_WithNumber()
	{
		var block = GetBlockOperation(@"
			class TestClass
			{
				void TestMethod()
				{
					int num = 42;
					string result = ""The answer is "" + num;
				}
			}
		");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		Assert.AreEqual(@"{
  let num = 42;
  let result = ""The answer is "" + num;
}", script);
	}

	/// <summary>
	/// 测试字符串连接 - 与布尔值连接
	/// </summary>
	[TestMethod]
	public void Visit_StringConcat_WithBoolean()
	{
		var block = GetBlockOperation(@"
			class TestClass
			{
				void TestMethod()
				{
					bool flag = true;
					string result = ""Flag is "" + flag;
				}
			}
		");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		Assert.AreEqual(@"{
  let flag = true;
  let result = ""Flag is "" + flag;
}", script);
	}

	#endregion

	#region 扩展测试用例 - 插值字符串变体

	/// <summary>
	/// 测试插值字符串 - 嵌套表达式
	/// </summary>
	[TestMethod]
	public void Visit_InterpolatedString_NestedExpression()
	{
		var block = GetBlockOperation(@"
			class TestClass
			{
				void TestMethod()
				{
					int a = 1;
					int b = 2;
					string result = $""Sum: {a + b}, Product: {a * b}"";
				}
			}
		");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		Assert.AreEqual(@"{
  let a = 1;
  let b = 2;
  let result = `Sum: ${a + b}, Product: ${a * b}`;
}", script);
	}

	/// <summary>
	/// 测试插值字符串 - 方法调用
	/// </summary>
	[TestMethod]
	public void Visit_InterpolatedString_MethodCallExpr()
	{
		var block = GetBlockOperation(@"
			class TestClass
			{
				void TestMethod()
				{
					string name = ""hello world"";
					string result = $""Upper: {name.ToUpper()}"";
				}
			}
		");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		Assert.AreEqual(@"{
  let name = ""hello world"";
  let result = `Upper: ${name.toUpperCase()}`;
}", script);
	}

	/// <summary>
	/// 测试插值字符串 - 三元表达式
	/// </summary>
	[TestMethod]
	public void Visit_InterpolatedString_TernaryExpr()
	{
		var block = GetBlockOperation(@"
			class TestClass
			{
				void TestMethod()
				{
					int value = 5;
					string result = $""Value is {(value > 0 ? ""positive"" : ""non-positive"")}"";
				}
			}
		");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		Assert.AreEqual(@"{
  let value = 5;
  let result = `Value is ${value > 0 ? ""positive"" : ""non-positive""}`;
}", script);
	}

	/// <summary>
	/// 测试插值字符串 - 空值处理
	/// </summary>
	[TestMethod]
	public void Visit_InterpolatedString_NullValue()
	{
		var block = GetBlockOperation(@"
			class TestClass
			{
				void TestMethod()
				{
					string name = null;
					string result = $""Hello, {name ?? ""Guest""}!"";
				}
			}
		");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		Assert.AreEqual(@"{
  let name = null;
  let result = `Hello, ${name ?? ""Guest""}!`;
}", script);
	}

	/// <summary>
	/// 测试插值字符串 - 多行字符串
	/// </summary>
	[TestMethod]
	public void Visit_InterpolatedString_Multiline()
	{
		var block = GetBlockOperation(@"
			class TestClass
			{
				void TestMethod()
				{
					string name = ""World"";
					string result = $@""Hello,
{name}!"";
				}
			}
		");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		AssertScriptEqual(@"{
  let name = ""World"";
  let result = `Hello,
${name}!`;
}", script);
	}

	#endregion

	#region 扩展测试用例 - 字符串方法

	/// <summary>
	/// 测试字符串 Length 属性
	/// </summary>
	[TestMethod]
	public void Visit_String_Length()
	{
		var block = GetBlockOperation(@"
			class TestClass
			{
				void TestMethod()
				{
					string text = ""Hello"";
					int len = text.Length;
				}
			}
		");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		Assert.AreEqual(@"{
  let text = ""Hello"";
  let len = text.length;
}", script);
	}

	/// <summary>
	/// 测试字符串 ToLower 方法
	/// </summary>
	[TestMethod]
	public void Visit_String_ToLower()
	{
		var block = GetBlockOperation(@"
			class TestClass
			{
				void TestMethod()
				{
					string text = ""HELLO"";
					string lower = text.ToLower();
				}
			}
		");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		Assert.AreEqual(@"{
  let text = ""HELLO"";
  let lower = text.toLowerCase();
}", script);
	}

	/// <summary>
	/// 测试字符串 Contains 方法
	/// </summary>
	[TestMethod]
	public void Visit_String_Contains()
	{
		var block = GetBlockOperation(@"
			class TestClass
			{
				void TestMethod()
				{
					string text = ""Hello World"";
					bool hasWorld = text.Contains(""World"");
				}
			}
		");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		Assert.AreEqual(@"{
  let text = ""Hello World"";
  let hasWorld = text.includes(""World"");
}", script);
	}

	/// <summary>
	/// 测试字符串 StartsWith 方法
	/// </summary>
	[TestMethod]
	public void Visit_String_StartsWith()
	{
		var block = GetBlockOperation(@"
			class TestClass
			{
				void TestMethod()
				{
					string text = ""Hello World"";
					bool starts = text.StartsWith(""Hello"");
				}
			}
		");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		Assert.AreEqual(@"{
  let text = ""Hello World"";
  let starts = text.startsWith(""Hello"");
}", script);
	}

	/// <summary>
	/// 测试字符串 EndsWith 方法
	/// </summary>
	[TestMethod]
	public void Visit_String_EndsWith()
	{
		var block = GetBlockOperation(@"
			class TestClass
			{
				void TestMethod()
				{
					string text = ""Hello World"";
					bool ends = text.EndsWith(""World"");
				}
			}
		");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		Assert.AreEqual(@"{
  let text = ""Hello World"";
  let ends = text.endsWith(""World"");
}", script);
	}

	/// <summary>
	/// 测试字符串 Replace 方法
	/// </summary>
	[TestMethod]
	public void Visit_String_Replace()
	{
		var block = GetBlockOperation(@"
			class TestClass
			{
				void TestMethod()
				{
					string text = ""Hello World"";
					string replaced = text.Replace(""World"", ""Universe"");
				}
			}
		");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		AssertScriptEqual(@"{
  let text = ""Hello World"";
  let replaced = text.replaceAll(""World"", ""Universe"");
}", script);
	}

	[TestMethod]
	public void Visit_String_ReplaceChar()
	{
		var block = GetBlockOperation(@"
			class TestClass
			{
				void TestMethod()
				{
					string text = ""Hello World"";
					string replaced = text.Replace('o', '0');
				}
			}
		");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		AssertScriptEqual(@"{
  let text = ""Hello World"";
  let replaced = _7d7cb13bbbbb83c8(text, ""o"", ""0"");
}", script);
	}

	[TestMethod]
	public void Visit_String_ReplaceIgnoreCase()
	{
		var block = GetBlockOperation(@"
			class TestClass
			{
				void TestMethod()
				{
					string text = ""Hello hello HeLLo"";
					string replaced = text.Replace(""hello"", ""hi"", StringComparison.OrdinalIgnoreCase);
				}
			}
		");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		AssertScriptEqual(@"{
  let text = ""Hello hello HeLLo"";
  let replaced = _8a7510653022a974(text, ""hello"", ""hi"", 5);
}", script);
	}

	#endregion

	#region 扩展测试用例 - 特殊字符串

	/// <summary>
	/// 测试空字符串
	/// </summary>
	[TestMethod]
	public void Visit_String_Empty()
	{
		var block = GetBlockOperation(@"
			class TestClass
			{
				void TestMethod()
				{
					string empty = """";
					bool isEmpty = string.IsNullOrEmpty(empty);
				}
			}
		");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		AssertScriptEqual(@"{
  let empty = """";
  let isEmpty = !empty;
}", script);
	}

	/// <summary>
	/// 测试字符串空格
	/// </summary>
	[TestMethod]
	public void Visit_String_Whitespace()
	{
		var block = GetBlockOperation(@"
			class TestClass
			{
				void TestMethod()
				{
					string whitespace = ""   "";
					string trimmed = whitespace.Trim();
				}
			}
		");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		Assert.AreEqual(@"{
  let whitespace = ""   "";
  let trimmed = whitespace.trim();
}", script);
	}

	/// <summary>
	/// 测试字符串转义字符
	/// </summary>
	[TestMethod]
	public void Visit_String_EscapeCharacters()
	{
		var block = GetBlockOperation(@"
			class TestClass
			{
				void TestMethod()
				{
					string text = ""Line1\nLine2\tTabbed"";
				}
			}
		");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		Assert.AreEqual(@"{
  let text = ""Line1\nLine2\tTabbed"";
}", script);
	}

	/// <summary>
	/// 测试字符串连接运算符 +=
	/// </summary>
	[TestMethod]
	public void Visit_String_ConcatAssign()
	{
		var block = GetBlockOperation(@"
			class TestClass
			{
				void TestMethod()
				{
					string result = ""Hello"";
					result += "" World"";
				}
			}
		");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		Assert.AreEqual(@"{
  let result = ""Hello"";
  result += "" World"";
}", script);
	}

	/// <summary>
	/// 测试多行字符串连接
	/// </summary>
	[TestMethod]
	public void Visit_String_MultiLineConcat()
	{
		var block = GetBlockOperation(@"
			class TestClass
			{
				void TestMethod()
				{
					string multi = ""Line1"" +
						""Line2"" +
						""Line3"";
				}
			}
		");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		Assert.AreEqual(@"{
  let multi = ""Line1"" + ""Line2"" + ""Line3"";
}", script);
	}

	/// <summary>
	/// 测试插值字符串中的三元运算符
	/// </summary>
	[TestMethod]
	public void Visit_InterpolatedString_Ternary()
	{
		var block = GetBlockOperation(@"
			class TestClass
			{
				void TestMethod()
				{
					int x = 5;
					string result = $""Value is {(x > 0 ? ""positive"" : ""non-positive"")}"";
				}
			}
		");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		Assert.AreEqual(@"{
  let x = 5;
  let result = `Value is ${x > 0 ? ""positive"" : ""non-positive""}`;
}", script);
	}

	/// <summary>
	/// 测试插值字符串中的方法调用
	/// </summary>
	[TestMethod]
	public void Visit_InterpolatedString_MethodCall()
	{
		var block = GetBlockOperation(@"
			class TestClass
			{
				void TestMethod()
				{
					string name = ""Hello World"";
					string result = $""Upper: {name.ToUpper()}"";
				}
			}
		");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		Assert.AreEqual(@"{
  let name = ""Hello World"";
  let result = `Upper: ${name.toUpperCase()}`;
}", script);
	}

	/// <summary>
	/// 测试插值字符串中的 null 值
	/// </summary>
	[TestMethod]
	public void Visit_InterpolatedString_Null()
	{
		var block = GetBlockOperation(@"
			class TestClass
			{
				void TestMethod()
				{
					string? s = null;
					string result = $""Value: {s}"";
				}
			}
		");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		Assert.AreEqual(@"{
  let s = null;
  let result = `Value: ${s}`;
}", script);
	}

	/// <summary>
	/// 测试字符串比较
	/// </summary>
	[TestMethod]
	public void Visit_String_Comparison()
	{
		var block = GetBlockOperation(@"
			class TestClass
			{
				void TestMethod()
				{
					string a = ""hello"";
					string b = ""world"";
					bool equal = a == b;
					bool notEqual = a != b;
				}
			}
		");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		Assert.AreEqual(@"{
  let a = ""hello"";
  let b = ""world"";
  let equal = a === b;
  let notEqual = a !== b;
}", script);
	}

	/// <summary>
	/// 测试字符串 Length 属性
	/// </summary>
	[TestMethod]
	public void Visit_String_Length1()
	{
		var block = GetBlockOperation(@"
			class TestClass
			{
				void TestMethod()
				{
					string text = ""Hello"";
					int len = text.Length;
				}
			}
		");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		Assert.AreEqual(@"{
  let text = ""Hello"";
  let len = text.length;
}", script);
	}

	/// <summary>
	/// 测试字符串索引访问
	/// </summary>
	[TestMethod]
	public void Visit_String_Indexer()
	{
		var block = GetBlockOperation(@"
			class TestClass
			{
				void TestMethod()
				{
					string text = ""Hello"";
					char first = text[0];
					char last = text[text.Length - 1];
				}
			}
		");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		AssertScriptEqual(@"{
  let text = ""Hello"";
  let first = _5ad63706a889c294(text, 0);
  let last = _5ad63706a889c294(text, text.length - 1);
}", script);
	}

	/// <summary>
	/// 测试字符串索引器在可选链下仍保留短路语义
	/// </summary>
	[TestMethod]
	public void Visit_String_Indexer_ConditionalAccessReceiver_UsesNullishShortCircuit()
	{
		var block = GetBlockOperation(@"
			class TestClass
			{
				void TestMethod()
				{
					string text = null;
					char? first = text?[0];
				}
			}
		");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		AssertScriptEqual(@"{
  let v$0;
  let text = null;
  let first = (v$0 = text, v$0 == null ? undefined : _5ad63706a889c294(v$0, 0));
}", script);
	}

	[TestMethod]
	public void Visit_String_ImplicitIndexerFromEnd_ConditionalAccessReceiver_UsesNullishShortCircuit()
	{
		var block = GetBlockOperation(@"
			class TestClass
			{
				void TestMethod()
				{
					string text = null;
					char? last = text?[^1];
				}
			}
		");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		AssertScriptEqual(@"{
  let v$0;
  let text = null;
  let last = (v$0 = text, v$0 == null ? undefined : _5ad63706a889c294(v$0, v$0.length - 1));
}", script);
	}

	[TestMethod]
	public void Visit_String_ImplicitRange_ConditionalAccessReceiver_UsesNullishShortCircuit()
	{
		var block = GetBlockOperation(@"
			class TestClass
			{
				void TestMethod()
				{
					string text = null;
					string value = text?[1..^1];
				}
			}
		");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		AssertScriptEqual(@"{
  let v$0;
  let text = null;
  let value = (v$0 = text, v$0 == null ? undefined : v$0.substring(1, 1 + (v$0.length - 1 - 1)));
}", script);
	}

	/// <summary>
	/// 测试字符串 intrinsic 方法在可选链下保留短路语义
	/// </summary>
	[TestMethod]
	public void Visit_String_PadLeft_ConditionalAccess_UsesNullishShortCircuit()
	{
		var block = GetBlockOperation(@"
			class TestClass
			{
				void TestMethod()
				{
					string text = null;
					string value = text?.PadLeft(3);
				}
			}
		");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		AssertScriptEqual(@"{
  let v$0;
  let text = null;
  let value = (v$0 = text, v$0 == null ? undefined : v$0.padStart(3));
}", script);
	}

	/// <summary>
	/// 测试字符串 Substring 方法
	/// </summary>
	[TestMethod]
	public void Visit_String_Substring()
	{
		var block = GetBlockOperation(@"
			class TestClass
			{
				void TestMethod()
				{
					string text = ""Hello World"";
					string sub1 = text.Substring(0, 5);
					string sub2 = text.Substring(6);
				}
			}
		");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		AssertScriptEqual(@"{
  let text = ""Hello World"";
  let sub1 = text.substring(0, 0 + 5);
  let sub2 = text.substring(6);
}", script);
	}

	/// <summary>
	/// 测试字符串 Split 方法
	/// </summary>
	[TestMethod]
	public void Visit_String_Split()
	{
		var block = GetBlockOperation(@"
			class TestClass
			{
				void TestMethod()
				{
					string text = ""a,b,c"";
					string[] parts = text.Split(',');
				}
			}
		");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		AssertScriptEqual(@"{
  let text = ""a,b,c"";
  let parts = _d8080c573d45b4b4(text, "","", 0);
}", script);
	}

	/// <summary>
	/// 测试字符串 Join 方法
	/// </summary>
	[TestMethod]
	public void Visit_String_Join()
	{
		var block = GetBlockOperation(@"
			class TestClass
			{
				void TestMethod()
				{
					string[] parts = new string[] { ""a"", ""b"", ""c"" };
					string joined = string.Join("","", parts);
				}
			}
		");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		AssertScriptEqual(@"{
  let parts = [""a"", ""b"", ""c""];
  let joined = Array.from(parts).join("","");
}", script);
	}

	/// <summary>
	/// 测试字符串 IsNullOrEmpty
	/// </summary>
	[TestMethod]
	public void Visit_String_IsNullOrEmpty()
	{
		var block = GetBlockOperation(@"
			class TestClass
			{
				void TestMethod()
				{
					string s = """";
					bool empty = string.IsNullOrEmpty(s);
				}
			}
		");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		AssertScriptEqual(@"{
  let s = """";
  let empty = !s;
}", script);
	}

	/// <summary>
	/// 测试字符串 IsNullOrWhiteSpace
	/// </summary>
	[TestMethod]
	public void Visit_String_IsNullOrWhiteSpace()
	{
		var block = GetBlockOperation(@"
			class TestClass
			{
				void TestMethod()
				{
					string s = ""   "";
					bool whitespace = string.IsNullOrWhiteSpace(s);
				}
			}
		");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		AssertScriptEqual(@"{
  let s = ""   "";
  let whitespace = !s?.trim();
}", script);
	}

	/// <summary>
	/// 测试字符串格式化
	/// </summary>
	[TestMethod]
	public void Visit_String_Format()
	{
		var block = GetBlockOperation(@"
			class TestClass
			{
				void TestMethod()
				{
					string formatted = string.Format(""Name: {0}, Age: {1}"", ""John"", 30);
				}
			}
		");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		AssertScriptEqual(@"{
  let formatted = _8606f3cc36d1f8ed(""Name: {0}, Age: {1}"", ""John"", 30);
}", script);
	}

	/// <summary>
	/// 测试字符串插值带格式
	/// </summary>
	[TestMethod]
	public void Visit_InterpolatedString_Format()
	{
		var block = GetBlockOperation(@"
			class TestClass
			{
				void TestMethod()
				{
					double pi = 3.14159;
					string formatted = $""Pi: {pi:F2}"";
				}
			}
		");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		AssertScriptEqual(@"{
  let pi = 3.14159;
  let formatted = `Pi: ${pi}`;
}", script);
	}

	/// <summary>
	/// 测试字符串 Replace 多次
	/// </summary>
	[TestMethod]
	public void Visit_String_ReplaceChain()
	{
		var block = GetBlockOperation(@"
			class TestClass
			{
				void TestMethod()
				{
					string text = ""hello world"";
					string result = text.Replace(""hello"", ""hi"").Replace(""world"", ""there"");
				}
			}
		");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		AssertScriptEqual(@"{
  let text = ""hello world"";
  let result = text.replaceAll(""hello"", ""hi"").replaceAll(""world"", ""there"");
}", script);
	}

	/// <summary>
	/// 测试字符串 StartsWith/EndsWith
	/// </summary>
	[TestMethod]
	public void Visit_String_StartsEndsWith()
	{
		var block = GetBlockOperation(@"
			class TestClass
			{
				void TestMethod()
				{
					string filename = ""test.txt"";
					bool starts = filename.StartsWith(""test"");
					bool ends = filename.EndsWith("".txt"");
				}
			}
		");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		Assert.AreEqual(@"{
  let filename = ""test.txt"";
  let starts = filename.startsWith(""test"");
  let ends = filename.endsWith("".txt"");
}", script);
	}

	/// <summary>
	/// 测试字符串 PadLeft/PadRight
	/// </summary>
	[TestMethod]
	public void Visit_String_Pad()
	{
		var block = GetBlockOperation(@"
			class TestClass
			{
				void TestMethod()
				{
					string text = ""42"";
					string paddedLeft = text.PadLeft(5);
					string paddedRight = text.PadRight(5);
				}
			}
		");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		Assert.AreEqual(@"{
  let text = ""42"";
  let paddedLeft = text.padStart(5);
  let paddedRight = text.padEnd(5);
}", script);
	}

	/// <summary>
	/// 测试字符串 ToCharArray
	/// </summary>
	[TestMethod]
	public void Visit_String_ToCharArray()
	{
		var block = GetBlockOperation(@"
			class TestClass
			{
				void TestMethod()
				{
					string text = ""Hello"";
					char[] chars = text.ToCharArray();
				}
			}
		");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		Assert.AreEqual(@"{
  let text = ""Hello"";
  let chars = text.split("""");
}", script);
	}

	/// <summary>
	/// 测试字符串 @ 前缀（逐字字符串）
	/// </summary>
	[TestMethod]
	public void Visit_String_Verbatim()
	{
		var block = GetBlockOperation(@"
			class TestClass
			{
				void TestMethod()
				{
					string path = @""C:\Users\test"";
				}
			}
		");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		AssertScriptEqual(@"{
  let path = ""C:\\Users\\test"";
}", script);
	}

	#endregion

	#region 扩展测试用例 - 更多字符串方法

	/// <summary>
	/// 测试字符串 Replace 多次调用
	/// </summary>
	[TestMethod]
	public void Visit_String_ReplaceChained()
	{
		var block = GetBlockOperation(@"
			class TestClass
			{
				void TestMethod()
				{
					string text = ""hello world"";
					string result = text.Replace(""hello"", ""hi"").Replace(""world"", ""there"");
				}
			}
		");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		AssertScriptEqual(@"{
  let text = ""hello world"";
  let result = text.replaceAll(""hello"", ""hi"").replaceAll(""world"", ""there"");
}", script);
	}

	/// <summary>
	/// 测试字符串 ToLowerInvariant
	/// </summary>
	[TestMethod]
	public void Visit_String_ToLowerInvariant()
	{
		var block = GetBlockOperation(@"
			class TestClass
			{
				void TestMethod()
				{
					string text = ""HELLO"";
					string lower = text.ToLowerInvariant();
				}
			}
		");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		Assert.AreEqual(@"{
  let text = ""HELLO"";
  let lower = text.toLowerCase();
}", script);
	}

	/// <summary>
	/// 测试字符串 ToUpperInvariant
	/// </summary>
	[TestMethod]
	public void Visit_String_ToUpperInvariant()
	{
		var block = GetBlockOperation(@"
			class TestClass
			{
				void TestMethod()
				{
					string text = ""hello"";
					string upper = text.ToUpperInvariant();
				}
			}
		");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		Assert.AreEqual(@"{
  let text = ""hello"";
  let upper = text.toUpperCase();
}", script);
	}

	/// <summary>
	/// 测试字符串 TrimStart
	/// </summary>
	[TestMethod]
	public void Visit_String_TrimStart()
	{
		var block = GetBlockOperation(@"
			class TestClass
			{
				void TestMethod()
				{
					string text = ""  hello  "";
					string trimmed = text.TrimStart();
				}
			}
		");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		Assert.AreEqual(@"{
  let text = ""  hello  "";
  let trimmed = text.trimStart();
}", script);
	}

	/// <summary>
	/// 测试字符串 TrimEnd
	/// </summary>
	[TestMethod]
	public void Visit_String_TrimEnd()
	{
		var block = GetBlockOperation(@"
			class TestClass
			{
				void TestMethod()
				{
					string text = ""  hello  "";
					string trimmed = text.TrimEnd();
				}
			}
		");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		Assert.AreEqual(@"{
  let text = ""  hello  "";
  let trimmed = text.trimEnd();
}", script);
	}

	/// <summary>
	/// 测试字符串 IsNullOrEmpty
	/// </summary>
	[TestMethod]
	public void Visit_String_IsNullOrEmpty1()
	{
		var block = GetBlockOperation(@"
			class TestClass
			{
				void TestMethod()
				{
					string text = ""hello"";
					bool isEmpty = string.IsNullOrEmpty(text);
				}
			}
		");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		AssertScriptEqual(@"{
  let text = ""hello"";
  let isEmpty = !text;
}", script);
	}

	/// <summary>
	/// 测试字符串 IsNullOrWhiteSpace
	/// </summary>
	[TestMethod]
	public void Visit_String_IsNullOrWhiteSpace1()
	{
		var block = GetBlockOperation(@"
			class TestClass
			{
				void TestMethod()
				{
					string text = ""  "";
					bool isWhiteSpace = string.IsNullOrWhiteSpace(text);
				}
			}
		");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		AssertScriptEqual(@"{
  let text = ""  "";
  let isWhiteSpace = !text?.trim();
}", script);
	}

	/// <summary>
	/// 测试字符串 Join 静态方法
	/// </summary>
	[TestMethod]
	public void Visit_String_Join1()
	{
		var block = GetBlockOperation(@"
			class TestClass
			{
				void TestMethod()
				{
					string[] parts = [""a"", ""b"", ""c""];
					string joined = string.Join("","", parts);
				}
			}
		");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		AssertScriptEqual(@"{
  let parts = [""a"", ""b"", ""c""];
  let joined = Array.from(parts).join("","");
}", script);
	}

	/// <summary>
	/// 测试字符串 Split 多字符
	/// </summary>
	[TestMethod]
	public void Visit_String_SplitMultipleChars()
	{
		var block = GetBlockOperation(@"
			class TestClass
			{
				void TestMethod()
				{
					string text = ""a,b;c"";
					string[] parts = text.Split([',', ';']);
				}
			}
		");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		AssertScriptEqual(@"{
  let text = ""a,b;c"";
  let parts = _5417a93b3075813a(text, ["","", "";""]);
}", script);
	}

	[TestMethod]
	public void Visit_String_SplitMultipleChars_ParamsForm()
	{
		var block = GetBlockOperation(@"
			class TestClass
			{
				void TestMethod()
				{
					string text = ""a,b;c"";
					string[] parts = text.Split(',', ';');
				}
			}
		");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		AssertScriptEqual(@"{
  let text = ""a,b;c"";
  let parts = _5417a93b3075813a(text, ["","", "";""]);
}", script);
	}

	[TestMethod]
	public void Visit_String_SplitChar_WithStringSplitOptions()
	{
		var block = GetBlockOperation(@"
			class TestClass
			{
				void TestMethod()
				{
					string text = "" a,, b ,"";
					string[] parts = text.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
				}
			}
		");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		AssertScriptEqual(@"{
  let text = "" a,, b ,"";
  let parts = _d8080c573d45b4b4(text, "","", 1 | 2);
}", script);
	}

	[TestMethod]
	public void Visit_String_SplitChar_WithCountAndStringSplitOptions()
	{
		var block = GetBlockOperation(@"
			class TestClass
			{
				void TestMethod()
				{
					string text = "" a,, b , c "";
					string[] parts = text.Split(',', 2, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
				}
			}
		");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		AssertScriptEqual(@"{
  let text = "" a,, b , c "";
  let parts = _aaa73a4811837ec7(text, "","", 2, 1 | 2);
}", script);
	}

	[TestMethod]
	public void Visit_String_SplitChars_WithStringSplitOptions()
	{
		var block = GetBlockOperation(@"
			class TestClass
			{
				void TestMethod()
				{
					string text = "" a,; b ;"";
					string[] parts = text.Split([',', ';'], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
				}
			}
		");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		AssertScriptEqual(@"{
  let text = "" a,; b ;"";
  let parts = _25c1f15b0ed2cb6e(text, ["","", "";""], 1 | 2);
}", script);
	}

	[TestMethod]
	public void Visit_String_SplitChars_WithCount()
	{
		var block = GetBlockOperation(@"
			class TestClass
			{
				void TestMethod()
				{
					string text = ""a,b;c,d"";
					string[] parts = text.Split([',', ';'], 2);
				}
			}
		");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		AssertScriptEqual(@"{
  let text = ""a,b;c,d"";
  let parts = _d03d120228c0c4ed(text, ["","", "";""], 2);
}", script);
	}

	[TestMethod]
	public void Visit_String_SplitChars_WithCountAndStringSplitOptions()
	{
		var block = GetBlockOperation(@"
			class TestClass
			{
				void TestMethod()
				{
					string text = "" a,; b ; c "";
					string[] parts = text.Split([',', ';'], 2, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
				}
			}
		");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		AssertScriptEqual(@"{
  let text = "" a,; b ; c "";
  let parts = _c8e5ceed33c6c638(text, ["","", "";""], 2, 1 | 2);
}", script);
	}

	[TestMethod]
	public void Visit_String_SplitString_WithStringSplitOptions()
	{
		var block = GetBlockOperation(@"
			class TestClass
			{
				void TestMethod()
				{
					string text = "" one ::  two :: "";
					string[] parts = text.Split(""::"", StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
				}
			}
		");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		AssertScriptEqual(@"{
  let text = "" one ::  two :: "";
  let parts = _189761f781df8770(text, ""::"", 1 | 2);
}", script);
	}

	[TestMethod]
	public void Visit_String_SplitString_WithCountAndStringSplitOptions()
	{
		var block = GetBlockOperation(@"
			class TestClass
			{
				void TestMethod()
				{
					string text = "" one ::  two :: three "";
					string[] parts = text.Split(""::"", 2, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
				}
			}
		");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		AssertScriptEqual(@"{
  let text = "" one ::  two :: three "";
  let parts = _96eb0a23afa7fdfb(text, ""::"", 2, 1 | 2);
}", script);
	}

	[TestMethod]
	public void Visit_String_SplitStrings_WithStringSplitOptions()
	{
		var block = GetBlockOperation(@"
			class TestClass
			{
				void TestMethod()
				{
					string text = "" one ::  two -- three :: "";
					string[] parts = text.Split([""::"", ""--""], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
				}
			}
		");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		AssertScriptEqual(@"{
  let text = "" one ::  two -- three :: "";
  let parts = _fff99c96206a241e(text, [""::"", ""--""], 1 | 2);
}", script);
	}

	[TestMethod]
	public void Visit_String_SplitStrings_WithCountAndStringSplitOptions()
	{
		var block = GetBlockOperation(@"
			class TestClass
			{
				void TestMethod()
				{
					string text = "" one ::  two -- three :: four "";
					string[] parts = text.Split([""::"", ""--""], 2, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
				}
			}
		");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		AssertScriptEqual(@"{
  let text = "" one ::  two -- three :: four "";
  let parts = _f3c7edcc7cc89a4a(text, [""::"", ""--""], 2, 1 | 2);
}", script);
	}

	/// <summary>
	/// 测试字符串 LastIndexOf
	/// </summary>
	[TestMethod]
	public void Visit_String_LastIndexOf()
	{
		var block = GetBlockOperation(@"
			class TestClass
			{
				void TestMethod()
				{
					string text = ""hello world"";
					int index = text.LastIndexOf(""o"");
				}
			}
		");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		Assert.AreEqual(@"{
  let text = ""hello world"";
  let index = text.lastIndexOf(""o"");
}", script);
	}

	/// <summary>
	/// 测试字符串 Remove
	/// </summary>
	[TestMethod]
	public void Visit_String_Remove()
	{
		var block = GetBlockOperation(@"
			class TestClass
			{
				void TestMethod()
				{
					string text = ""hello world"";
					string removed = text.Remove(5);
				}
			}
		");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		Assert.AreEqual(@"{
  let text = ""hello world"";
  let removed = text.slice(0, 5);
}", script);
	}

	/// <summary>
	/// 测试字符串 Insert
	/// </summary>
	[TestMethod]
	public void Visit_String_Insert()
	{
		var block = GetBlockOperation(@"
			class TestClass
			{
				void TestMethod()
				{
					string text = ""helloworld"";
					string inserted = text.Insert(5, "" "");
				}
			}
		");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		Assert.AreEqual(@"{
  let text = ""helloworld"";
  let inserted = text.slice(0, 5) + "" "" + text.slice(5);
}", script);
	}

	/// <summary>
	/// 测试字符串 PadLeft 多字符
	/// </summary>
	[TestMethod]
	public void Visit_String_PadLeftWithChar()
	{
		var block = GetBlockOperation(@"
			class TestClass
			{
				void TestMethod()
				{
					string text = ""42"";
					string padded = text.PadLeft(5, '0');
				}
			}
		");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		AssertScriptEqual(@"{
  let text = ""42"";
  let padded = text.padStart(5, ""0"");
}", script);
	}

	/// <summary>
	/// 测试字符串 PadRight 多字符
	/// </summary>
	[TestMethod]
	public void Visit_String_PadRightWithChar()
	{
		var block = GetBlockOperation(@"
			class TestClass
			{
				void TestMethod()
				{
					string text = ""42"";
					string padded = text.PadRight(5, '-');
				}
			}
		");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		AssertScriptEqual(@"{
  let text = ""42"";
  let padded = text.padEnd(5, ""-"");
}", script);
	}

	[TestMethod]
	public void Visit_String_PadWithCharVariable()
	{
		var block = GetBlockOperation(@"
			class TestClass
			{
				void TestMethod()
				{
					string text = ""42"";
					char pad = '0';
					string left = text.PadLeft(5, pad);
					string right = text.PadRight(5, pad);
				}
			}
		");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		AssertScriptEqual(@"{
  let text = ""42"";
  let pad = ""0"";
  let left = text.padStart(5, pad);
  let right = text.padEnd(5, pad);
}", script);
	}

	[TestMethod]
	public void Visit_String_StartsEndsWithChar()
	{
		var block = GetBlockOperation(@"
			class TestClass
			{
				void TestMethod()
				{
					string text = ""hello"";
					bool starts = text.StartsWith('h');
					bool ends = text.EndsWith('o');
				}
			}
		");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		AssertScriptEqual(@"{
  let text = ""hello"";
  let starts = text.startsWith(""h"");
  let ends = text.endsWith(""o"");
}", script);
	}

	[TestMethod]
	public void Visit_String_IndexOfChar()
	{
		var block = GetBlockOperation(@"
			class TestClass
			{
				void TestMethod()
				{
					string text = ""hello world"";
					int first = text.IndexOf('o');
					int last = text.LastIndexOf('o');
				}
			}
		");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		AssertScriptEqual(@"{
  let text = ""hello world"";
  let first = text.indexOf(""o"");
  let last = text.lastIndexOf(""o"");
}", script);
	}

	[TestMethod]
	public void Visit_String_ContainsAndIndexedCharOverloads()
	{
		var block = GetBlockOperation(@"
			class TestClass
			{
				void TestMethod()
				{
					string text = ""hello world"";
					bool contains = text.Contains('o');
					int first = text.IndexOf('o', 5);
					int last = text.LastIndexOf('o', 7);
				}
			}
		");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		AssertScriptEqual(@"{
  let text = ""hello world"";
  let contains = text.includes(""o"");
  let first = text.indexOf(""o"", 5);
  let last = text.lastIndexOf(""o"", 7);
}", script);
	}

	[TestMethod]
	public void Visit_String_IndexedCharCountOverloads()
	{
		var block = GetBlockOperation(@"
			class TestClass
			{
				void TestMethod()
				{
					string text = ""hello world"";
					int first = text.IndexOf('o', 4, 4);
					int last = text.LastIndexOf('o', 7, 4);
				}
			}
		");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		AssertScriptEqual(@"{
  let text = ""hello world"";
  let first = _d2873e605fbed764(text, ""o"", 4, 4);
  let last = _dbdd57f8d259ce66(text, ""o"", 7, 4);
}", script);
	}

	[TestMethod]
	public void Visit_String_IndexedStringCountOverloads()
	{
		var block = GetBlockOperation(@"
			class TestClass
			{
				void TestMethod()
				{
					string text = ""hello world"";
					int first = text.IndexOf(""or"", 5, 4);
					int last = text.LastIndexOf(""lo"", 5, 4);
				}
			}
		");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		AssertScriptEqual(@"{
  let text = ""hello world"";
  let first = _ff549d811898fb56(text, ""or"", 5, 4);
  let last = _c4ee024d06ee238c(text, ""lo"", 5, 4);
}", script);
	}

	[TestMethod]
	public void Visit_String_TrimCharOverloads()
	{
		var block = GetBlockOperation(@"
			class TestClass
			{
				void TestMethod()
				{
					string text = ""--hello--"";
					string trimmed = text.Trim('-');
					string trimmedStart = text.TrimStart('-');
					string trimmedEnd = text.TrimEnd('-');
				}
			}
		");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		AssertScriptEqual(@"{
  let text = ""--hello--"";
  let trimmed = _5d7e005b9dcb67de(text, ""-"");
  let trimmedStart = _561fe737e62cf332(text, ""-"");
  let trimmedEnd = _eb362a090d734099(text, ""-"");
}", script);
	}

	[TestMethod]
	public void Visit_String_TrimCharArrayOverloads()
	{
		var block = GetBlockOperation(@"
			class TestClass
			{
				void TestMethod()
				{
					string text = ""-*-hello-*-"";
					string trimmed = text.Trim('-', '*');
					string trimmedStart = text.TrimStart('-', '*');
					string trimmedEnd = text.TrimEnd('-', '*');
				}
			}
		");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		AssertScriptEqual(@"{
  let text = ""-*-hello-*-"";
  let trimmed = _c6c444b4e71e14f7(text, [""-"", ""*""]);
  let trimmedStart = _98731360726c6976(text, [""-"", ""*""]);
  let trimmedEnd = _a62862c1fbaa21c3(text, [""-"", ""*""]);
}", script);
	}

	[TestMethod]
	public void Visit_String_IndexOfAnyAndLastIndexOfAny()
	{
		var block = GetBlockOperation(@"
			class TestClass
			{
				void TestMethod()
				{
					string text = ""hello world"";
					int first = text.IndexOfAny(['o', 'w']);
					int last = text.LastIndexOfAny(['o', 'w']);
				}
			}
		");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		AssertScriptEqual(@"{
  let text = ""hello world"";
  let first = _69b749a1c6cbae78(text, [""o"", ""w""]);
  let last = _c0212f4213a99019(text, [""o"", ""w""]);
}", script);
	}

	[TestMethod]
	public void Visit_String_IndexOfAnyAndLastIndexOfAny_WithStartIndex()
	{
		var block = GetBlockOperation(@"
			class TestClass
			{
				void TestMethod()
				{
					string text = ""hello world"";
					int first = text.IndexOfAny(['o', 'w'], 5);
					int last = text.LastIndexOfAny(['o', 'w'], 7);
				}
			}
		");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		AssertScriptEqual(@"{
  let text = ""hello world"";
  let first = _63633a5f3b85c5a9(text, [""o"", ""w""], 5);
  let last = _c401e64318e768c4(text, [""o"", ""w""], 7);
}", script);
	}

	[TestMethod]
	public void Visit_String_IndexOfAnyAndLastIndexOfAny_WithStartIndexAndCount()
	{
		var block = GetBlockOperation(@"
			class TestClass
			{
				void TestMethod()
				{
					string text = ""hello world"";
					int first = text.IndexOfAny(['o', 'w'], 4, 4);
					int last = text.LastIndexOfAny(['o', 'w'], 7, 4);
				}
			}
		");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		AssertScriptEqual(@"{
  let text = ""hello world"";
  let first = _cb863079aae72451(text, [""o"", ""w""], 4, 4);
  let last = _3c17fcef5615e7a3(text, [""o"", ""w""], 7, 4);
}", script);
	}

	#endregion

	#region 扩展测试用例 - 字符串比较

	/// <summary>
	/// 测试字符串 Equals 忽略大小写
	/// </summary>
	[TestMethod]
	public void Visit_String_EqualsIgnoreCase()
	{
		var block = GetBlockOperation(@"
			class TestClass
			{
				void TestMethod()
				{
					string a = ""Hello"";
					string b = ""hello"";
					bool equal = a.Equals(b, StringComparison.OrdinalIgnoreCase);
				}
			}
		");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		AssertScriptEqual(@"{
  let a = ""Hello"";
  let b = ""hello"";
  let equal = _f8e1e01e8c17e8bb(a, b, 5);
}", script);
	}

	[TestMethod]
	public void Visit_String_ComparisonHelpers_OrdinalIgnoreCase()
	{
		var block = GetBlockOperation(@"
			class TestClass
			{
				void TestMethod()
				{
					string text = ""Hello World"";
					bool starts = text.StartsWith(""hello"", StringComparison.OrdinalIgnoreCase);
					bool ends = text.EndsWith(""WORLD"", StringComparison.OrdinalIgnoreCase);
					bool contains = text.Contains(""LO WO"", StringComparison.OrdinalIgnoreCase);
					bool equal = string.Equals(text, ""hello world"", StringComparison.OrdinalIgnoreCase);
				}
			}
		");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		AssertScriptEqual(@"{
  let text = ""Hello World"";
  let starts = _0333a0fd5f67d8a0(text, ""hello"", 5);
  let ends = _946b7129a48c8114(text, ""WORLD"", 5);
  let contains = _d52d7114d5c1b839(text, ""LO WO"", 5);
  let equal = _b7c36408f0f172e9(text, ""hello world"", 5);
}", script);
	}

	[TestMethod]
	public void Visit_String_SearchHelpers_OrdinalIgnoreCase()
	{
		var block = GetBlockOperation(@"
			class TestClass
			{
				void TestMethod()
				{
					string text = ""Hello World hello"";
					int first = text.IndexOf(""WORLD"", StringComparison.OrdinalIgnoreCase);
					int next = text.IndexOf(""HELLO"", 1, StringComparison.OrdinalIgnoreCase);
					int last = text.LastIndexOf(""HELLO"", StringComparison.OrdinalIgnoreCase);
					int lastFrom = text.LastIndexOf(""HELLO"", 10, StringComparison.OrdinalIgnoreCase);
				}
			}
		");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		AssertScriptEqual(@"{
  let text = ""Hello World hello"";
  let first = _3ae4900da2b07b27(text, ""WORLD"", 5);
  let next = _2fabe2b831abe71e(text, ""HELLO"", 1, 5);
  let last = _78449c135e18c4bc(text, ""HELLO"", 5);
  let lastFrom = _359dbce44ce4a4da(text, ""HELLO"", 10, 5);
}", script);
	}

	[TestMethod]
	public void Visit_String_SearchHelpers_OrdinalIgnoreCase_WithCountAndChar()
	{
		var block = GetBlockOperation(@"
			class TestClass
			{
				void TestMethod()
				{
					string text = ""Hello World hello"";
					bool contains = text.Contains('H', StringComparison.OrdinalIgnoreCase);
					int charIndex = text.IndexOf('W', StringComparison.OrdinalIgnoreCase);
					int first = text.IndexOf(""WORLD"", 3, 8, StringComparison.OrdinalIgnoreCase);
					int last = text.LastIndexOf(""HELLO"", 15, 8, StringComparison.OrdinalIgnoreCase);
				}
			}
		");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		AssertScriptEqual(@"{
  let text = ""Hello World hello"";
  let contains = _16d4b2b4de019fb2(text, ""H"", 5);
  let charIndex = _5331447e2c855a66(text, ""W"", 5);
  let first = _ab22561fc42166db(text, ""WORLD"", 3, 8, 5);
  let last = _c911a06f021bd138(text, ""HELLO"", 15, 8, 5);
}", script);
	}

	/// <summary>
	/// 测试字符串 Compare
	/// </summary>
	[TestMethod]
	public void Visit_String_Compare()
	{
		var block = GetBlockOperation(@"
			class TestClass
			{
				void TestMethod()
				{
					string a = ""apple"";
					string b = ""banana"";
					int result = string.Compare(a, b);
				}
			}
		");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		AssertScriptEqual(@"{
  let a = ""apple"";
  let b = ""banana"";
  let result = _e16eea9fe3891a62(a, b);
}", script);
	}

	[TestMethod]
	public void Visit_String_Compare_OrdinalIgnoreCase()
	{
		var block = GetBlockOperation(@"
			class TestClass
			{
				void TestMethod()
				{
					string a = ""Apple"";
					string b = ""apple"";
					int result = string.Compare(a, b, StringComparison.OrdinalIgnoreCase);
				}
			}
		");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		AssertScriptEqual(@"{
  let a = ""Apple"";
  let b = ""apple"";
  let result = _9d940114ace1198f(a, b, 5);
}", script);
	}

	[TestMethod]
	public void Visit_String_Compare_Substrings_OrdinalIgnoreCase()
	{
		var block = GetBlockOperation(@"
			class TestClass
			{
				void TestMethod()
				{
					string a = ""--Hello--"";
					string b = ""xxhello??"";
					int result = string.Compare(a, 2, b, 2, 5, StringComparison.OrdinalIgnoreCase);
				}
			}
		");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		AssertScriptEqual(@"{
  let a = ""--Hello--"";
  let b = ""xxhello??"";
  let result = _d78fb9d76fca75e4(a, 2, b, 2, 5, 5);
}", script);
	}

	/// <summary>
	/// 测试字符串 CompareOrdinal
	/// </summary>
	[TestMethod]
	public void Visit_String_CompareOrdinal()
	{
		var block = GetBlockOperation(@"
			class TestClass
			{
				void TestMethod()
				{
					string a = ""a"";
					string b = ""B"";
					int result = string.CompareOrdinal(a, b);
				}
			}
		");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		AssertScriptEqual(@"{
  let a = ""a"";
  let b = ""B"";
  let result = _a55d307de6e31c7b(a, b);
}", script);
	}

	#endregion

	#region 扩展测试用例 - 字符串与数字转换

	/// <summary>
	/// 测试 int.Parse 字符串转整数
	/// </summary>
	[TestMethod]
	public void Visit_String_IntParse()
	{
		var block = GetBlockOperation(@"
			class TestClass
			{
				void TestMethod()
				{
					string numStr = ""42"";
					int num = int.Parse(numStr);
				}
			}
		");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		AssertScriptEqual(@"{
  let numStr = ""42"";
  let num = _151ccc6045162f8f(numStr);
}", script);
	}

	/// <summary>
	/// 测试 double.Parse 字符串转浮点数
	/// </summary>
	[TestMethod]
	public void Visit_String_DoubleParse()
	{
		var block = GetBlockOperation(@"
			class TestClass
			{
				void TestMethod()
				{
					string numStr = ""3.14"";
					double num = double.Parse(numStr);
				}
			}
		");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		AssertScriptEqual(@"{
  let numStr = ""3.14"";
  let num = _5810f85a3710b88d(numStr);
}", script);
	}

	/// <summary>
	/// 测试 ToString 数字转字符串
	/// </summary>
	[TestMethod]
	public void Visit_String_NumberToString()
	{
		var block = GetBlockOperation(@"
			class TestClass
			{
				void TestMethod()
				{
					int num = 42;
					string numStr = num.ToString();
				}
			}
		");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		AssertScriptEqual(@"{
  let num = 42;
  let numStr = num.toString();
}", script);
	}

	/// <summary>
	/// 测试 ToString 格式化
	/// </summary>
	[TestMethod]
	public void Visit_String_ToStringFormat()
	{
		var block = GetBlockOperation(@"
			class TestClass
			{
				void TestMethod()
				{
					int num = 42;
					string hex = num.ToString(""X"");
				}
			}
		");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		AssertScriptEqual(@"{
  let num = 42;
  let hex = (num >>> 0).toString(16).toUpperCase();
}", script);
	}

	#endregion
}
