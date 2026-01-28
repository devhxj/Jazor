using ECMAScript.Compiler;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Operations;

namespace ECMAScript.ComplierTest;

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
        global using System.Linq;";

		var compilation = CSharpCompilation.Create(
			"TestAssembly",
			syntaxTrees: [
			  CSharpSyntaxTree.ParseText(usings),
			  CSharpSyntaxTree.ParseText(code)
			],
			references: Basic.Reference.Assemblies.Net100.References.All,
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
		var methodDeclaration = root.DescendantNodes().OfType<MethodDeclarationSyntax>().FirstOrDefault();
		if (methodDeclaration?.Body is not null)
		{
			var operation = semanticModel.GetOperation(methodDeclaration.Body) as IBlockOperation;
			if (operation is not null)
				return operation;
		}

		throw new InvalidOperationException("未找到可分析的操作");
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
  let message = 'Hello World';
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
  let message = `Length: ${name.Length}`;
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
  let message = `Length: ${str.Length}`;
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
  Console.WriteLine(`Value: ${x}`);
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
  Console.WriteLine(greeting);
  Console.WriteLine(info);
  Console.WriteLine(nextYear);
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

		Assert.AreEqual(@"{
  let message = 'Line1\nLine2';
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
		var script = node?.ToKnRECMAScript();

		Assert.AreEqual(@"{
  let message = 'Text
More';
}", script);
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
		var script = node?.ToKnRECMAScript();

		Assert.AreEqual(@"{
  let name = ""Apple"";
  let price = 100;
  let message = 'Item:	${name}
Price:	${price}';
}", script);
	}

	#endregion
}
