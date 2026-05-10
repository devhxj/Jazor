using ECMAScript;
using Jazor.Compiler;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Operations;

namespace Jazor.ComplierTest;

/// <summary>
/// SemanticWalker Switch 语句功能测试类
///
/// 本测试类验证 C# switch 语句到 JavaScript 的转换功能，涵盖：
/// - 传统 switch 语句（常量 case）
/// - 带 default 的 switch 语句
/// - Fallthrough 行为（多值共享同一个 body）
/// - 不同类型的字面量（int, string, bool）
/// - 模式匹配的 switch 语句（类型模式、关系模式）
/// - DefaultCaseClause 和 SingleValueCaseClause 的直接测试
///
/// 测试方法命名约定：
/// - VisitSwitch_[Scenario]: 传统 switch 语句测试
/// - VisitSwitch_PatternMatching_[Type]: 模式匹配 switch 测试
/// - VisitDefaultCaseClause/VisitSingleValueCaseClause: 直接方法调用测试
/// </summary>
[TestClass]
public sealed class SemanticWalkerSwitchTest
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

		// 优先定位约定的 TestMethod，避免前置辅助方法让测试误取错误的方法体。
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

	/// <summary>
	/// 获取指定索引的操作
	/// </summary>
	private static T GetOperationAt<T>(IBlockOperation block, int index = 0) where T : class, IOperation
	{
		var operation = block.Operations.Skip(index).First();
		return operation as T ?? throw new InvalidOperationException("未找到可分析的操作");
	}

	private static void AssertScriptEqual(string expected, string? actual)
		=> Assert.AreEqual(ExpectedJsNaming.Normalize(expected).ReplaceLineEndings("\n"), actual?.ReplaceLineEndings("\n"));

	// ==================== 传统 Switch 语句测试 ====================

	/// <summary>
	/// 测试基本的 switch 语句（单个 case）
	/// C# 示例：switch (value) { case 1: break; }
	/// 转换结果：JavaScript switch 语句
	/// </summary>
	[TestMethod]
	public void VisitSwitch_SingleCase()
	{
		var block = GetBlockOperation(@"
			class TestClass
			{
				void TestMethod()
				{
					int value = 1;
					switch (value)
					{
						case 1:
							break;
					}
				}
			}
		");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		Assert.AreEqual(
			@"{
  let value = 1;
  switch (value) {
    case 1:
      break;
  }
}", script);
	}

	/// <summary>
	/// 测试 switch 语句（多个 case）
	/// C# 示例：switch (value) { case 1: break; case 2: break; }
	/// 转换结果：JavaScript switch 语句
	/// </summary>
	[TestMethod]
	public void VisitSwitch_MultipleCases()
	{
		var block = GetBlockOperation(@"
			class TestClass
			{
				void TestMethod()
				{
					int value = 1;
					switch (value)
					{
						case 1:
							break;
						case 2:
							break;
						case 3:
							break;
					}
				}
			}
		");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		Assert.AreEqual(
			@"{
  let value = 1;
  switch (value) {
    case 1:
      break;
    case 2:
      break;
    case 3:
      break;
  }
}", script);
	}

	/// <summary>
	/// 测试 switch 语句（带 default）
	/// C# 示例：switch (value) { case 1: break; default: break; }
	/// 转换结果：JavaScript switch 语句
	/// 注意：空的 default case 会被省略
	/// </summary>
	[TestMethod]
	public void VisitSwitch_WithDefault()
	{
		var block = GetBlockOperation(@"
			class TestClass
			{
				void TestMethod()
				{
					int value = 1;
					switch (value)
					{
						case 1:
							break;
						default:
							break;
					}
				}
			}
		");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		// default case 会被保留（即使是空的）
		Assert.AreEqual(
			@"{
  let value = 1;
  switch (value) {
    case 1:
      break;
    default:
      break;
  }
}", script);
	}

	/// <summary>
	/// 测试 switch 语句（fallthrough - 多个值共享同一个 body）
	/// C# 示例：switch (value) { case 1: case 2: break; }
	/// 转换结果：JavaScript switch 语句，第二个 case 无 body
	/// </summary>
	[TestMethod]
	public void VisitSwitch_Fallthrough()
	{
		var block = GetBlockOperation(@"
			class TestClass
			{
				void TestMethod()
				{
					int value = 1;
					switch (value)
					{
						case 1:
						case 2:
							break;
					}
				}
			}
		");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		Assert.AreEqual(
			@"{
  let value = 1;
  switch (value) {
    case 1:
    case 2:
      break;
  }
}".ReplaceLineEndings(), script?.ReplaceLineEndings());
	}

	/// <summary>
	/// 测试 switch 语句（字符串字面量）
	/// C# 示例：switch (str) { case "hello": break; }
	/// 转换结果：JavaScript switch 语句
	/// </summary>
	[TestMethod]
	public void VisitSwitch_StringLiterals()
	{
		var block = GetBlockOperation(@"
			class TestClass
			{
				void TestMethod()
				{
					string str = ""hello"";
					switch (str)
					{
						case ""hello"":
							break;
						case ""world"":
							break;
					}
				}
			}
		");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		Assert.AreEqual(
			@"{
  let str = ""hello"";
  switch (str) {
    case ""hello"":
      break;
    case ""world"":
      break;
  }
}", script);
	}

	/// <summary>
	/// 测试 switch 语句（带实际执行语句）
	/// C# 示例：switch (value) { case 1: x = 1; break; }
	/// 转换结果：JavaScript switch 语句
	/// 注意：空的 default case 会被省略
	/// </summary>
	[TestMethod]
	public void VisitSwitch_WithStatements()
	{
		var block = GetBlockOperation(@"
			class TestClass
			{
				void TestMethod()
				{
					int value = 1;
					int result = 0;
					switch (value)
					{
						case 1:
							result = 100;
							break;
						case 2:
							result = 200;
							break;
						default:
							result = 0;
							break;
					}
				}
			}
		");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		// default case 的语句会被执行
		Assert.AreEqual(
			@"{
  let value = 1;
  let result = 0;
  switch (value) {
    case 1:
      result = 100;
      break;
    case 2:
      result = 200;
      break;
    default:
      result = 0;
      break;
  }
}", script);
	}

	/// <summary>
	/// 测试 switch 语句（布尔字面量）
	/// C# 示例：switch (flag) { case true: break; case false: break; }
	/// 转换结果：JavaScript switch 语句
	/// </summary>
	[TestMethod]
	public void VisitSwitch_BooleanLiterals()
	{
		var block = GetBlockOperation(@"
			class TestClass
			{
				void TestMethod()
				{
					bool flag = true;
					switch (flag)
					{
						case true:
							break;
						case false:
							break;
					}
				}
			}
		");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		Assert.AreEqual(
			@"{
  let flag = true;
  switch (flag) {
    case true:
      break;
    case false:
      break;
  }
}", script);
	}

	/// <summary>
	/// 测试 switch 语句（带方法调用）
	/// C# 示例：switch (value) { case 1: DoSomething(); break; }
	/// 转换结果：JavaScript switch 语句
	/// 注意：实例方法调用应保留在 switch 分支体内
	/// </summary>
	[TestMethod]
	public void VisitSwitch_WithMethodCalls()
	{
		var block = GetBlockOperation(@"
			class TestClass
			{
				void DoSomething() { }
				void DoOther() { }

				void TestMethod()
				{
					int value = 1;
					switch (value)
					{
						case 1:
							DoSomething();
							break;
						case 2:
							DoOther();
							break;
					}
				}
			}
		");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		AssertScriptEqual(
			@"{
  let value = 1;
  switch (value) {
    case 1:
      this.DoSomething();
      break;
    case 2:
      this.DoOther();
      break;
  }
}", script);
	}

	/// <summary>
	/// 测试 switch 语句（嵌套语句块）
	/// C# 示例：switch (value) { case 1: { int x = 1; } break; }
	/// 转换结果：JavaScript switch 语句
	/// 注意：嵌套块中的变量使用原始名称
	/// </summary>
	[TestMethod]
	public void VisitSwitch_NestedBlock()
	{
		var block = GetBlockOperation(@"
			class TestClass
			{
				void TestMethod()
				{
					int value = 1;
					switch (value)
					{
						case 1:
						{
							int x = 10;
							break;
						}
						case 2:
						{
							int y = 20;
							break;
						}
					}
				}
			}
		");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		Assert.AreEqual(
			@"{
  let value = 1;
  switch (value) {
    case 1: {
      let x = 10;
      break;
    }
    case 2: {
      let y = 20;
      break;
    }
  }
}", script);
	}

	/// <summary>
	/// 测试 switch 语句（仅 default）
	/// C# 示例：switch (value) { default: break; }
	/// 转换结果：JavaScript switch 语句
	/// 注意：空的 default case 会被省略
	/// </summary>
	[TestMethod]
	public void VisitSwitch_OnlyDefault()
	{
		var block = GetBlockOperation(@"
			class TestClass
			{
				void TestMethod()
				{
					int value = 1;
					switch (value)
					{
						default:
							break;
					}
				}
			}
		");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		// default case 会被保留
		Assert.AreEqual(
			@"{
  let value = 1;
  switch (value) {
    default:
      break;
  }
}", script);
	}

	// ==================== 模式匹配 Switch 测试 ====================

	/// <summary>
	/// 测试 switch 语句（类型模式）
	/// C# 示例：switch (obj) { case null: break; case string s: break; }
	/// 转换结果：IIFE + 独立 if 语句（因为包含类型模式）
	/// 注意：类型模式中的变量声明会被转换为赋值表达式，与类型检查组合在一起
	/// 注意：空 case 体（只有 break）被转换为 return 语句
	/// 注意：每个 case 转换为独立的 if 语句，而不是 else if 链
	/// </summary>
	[TestMethod]
	public void VisitSwitch_PatternMatching_TypePattern()
	{
		var block = GetBlockOperation(@"
			class TestClass
			{
				void TestMethod()
				{
					object obj = ""hello"";
					switch (obj)
					{
						case null:
							break;
						case string s:
							break;
						case int i:
							break;
					}
				}
			}
		");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		// 包含类型模式，应该转换为 IIFE + 独立 if 语句
		// 类型模式中的变量 s 和 i 只在 IIFE 内部可见，并通过赋值表达式进行初始化
		// 空 case 体被转换为 return 语句
		// 每个 case 是独立的 if 语句
		// 类型模式中的变量声明通过逗号表达式转换为赋值
		Assert.AreEqual(
@"{
  let obj = ""hello"";
  (() => {
    let s, i;
    const v$0 = obj;
    if (v$0 === null) {
      return;
    }
    if (typeof v$0 === ""string"" && (s = v$0, true)) {
      return;
    }
    if (typeof v$0 === ""number"" && (i = v$0, true)) {
      return;
    }
  })();
}", script);

	}

	[TestMethod]
	public void VisitSwitch_PatternMatching_AsyncCaseBody_UsesAsyncIife()
	{
		var block = GetBlockOperation(@"
			class TestClass
			{
				async System.Threading.Tasks.Task TestMethod()
				{
					object obj = ""hello"";
					switch (obj)
					{
						case string s:
							await System.Threading.Tasks.Task.CompletedTask;
							break;
						default:
							await System.Threading.Tasks.Task.CompletedTask;
							break;
					}
				}
			}
		");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		Assert.IsNotNull(script);
		StringAssert.Contains(script, "(async () => {", StringComparison.Ordinal);
		StringAssert.Contains(script, "await Promise.resolve();", StringComparison.Ordinal);
	}

	/// <summary>
	/// 测试 switch 语句（关系模式）
	/// C# 示例：switch (value) { case > 0: break; case < 0: break; }
	/// 转换结果：IIFE + 独立 if 语句
	/// 注意：空 case 体（只有 break）被转换为 return 语句
	/// 注意：每个 case 转换为独立的 if 语句，而不是 else if 链
	/// </summary>
	[TestMethod]
	public void VisitSwitch_PatternMatching_RelationalPattern()
	{
		var block = GetBlockOperation(@"
			class TestClass
			{
				void TestMethod()
				{
					int value = 5;
					switch (value)
					{
						case > 0:
							break;
						case < 0:
							break;
						case 0:
							break;
					}
				}
			}
		");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		Assert.AreEqual(
			@"{
  let value = 5;
  (() => {
    const v$0 = value;
    if (v$0 > 0) {
      return;
    }
    if (v$0 < 0) {
      return;
    }
    if (v$0 === 0) {
      return;
    }
  })();
}", script);
	}

	/// <summary>
	/// 测试 switch 语句（常量模式与关系模式混合）
	/// C# 示例：switch (value) { case 1: break; case > 1: break; }
	/// 转换结果：IIFE + 独立 if 语句（因为包含关系模式）
	/// 注意：空 case 体（只有 break）被转换为 return 语句
	/// 注意：每个 case 转换为独立的 if 语句，而不是 else if 链
	/// 注意：default case 直接输出语句，不包装在 if 中
	/// </summary>
	[TestMethod]
	public void VisitSwitch_PatternMatching_Mixed()
	{
		var block = GetBlockOperation(@"
			class TestClass
			{
				void TestMethod()
				{
					int value = 5;
					switch (value)
					{
						case 1:
							break;
						case 2:
							break;
						case > 2:
							break;
						default:
							break;
					}
				}
			}
		");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		Assert.AreEqual(
			@"{
  let value = 5;
  (() => {
    const v$0 = value;
    if (v$0 === 1) {
      return;
    }
    if (v$0 === 2) {
      return;
    }
    if (v$0 > 2) {
      return;
    }
    return;
  })();
}", script);
	}

	// ==================== DefaultCaseClause 测试 ====================

	/// <summary>
	/// 测试 VisitDefaultCaseClause 方法
	/// 应该返回 null
	/// </summary>
	[TestMethod]
	public void VisitDefaultCaseClause_ReturnsNull()
	{
		var block = GetBlockOperation(@"
			class TestClass
			{
				void TestMethod()
				{
					int value = 1;
					switch (value)
					{
						default:
							break;
					}
				}
			}
		");

		var walker = new SemanticWalker(true);
		var switchOp = GetOperationAt<ISwitchOperation>(block, 1);
		var defaultClause = switchOp.Cases[0].Clauses[0] as IDefaultCaseClauseOperation;

		var node = walker.VisitDefaultCaseClause(defaultClause!, new());

		Assert.IsNull(node);
	}

	// ==================== SingleValueCaseClause 测试 ====================

	/// <summary>
	/// 测试 VisitSingleValueCaseClause 方法（整数字面量）
	/// </summary>
	[TestMethod]
	public void VisitSingleValueCaseClause_Integer()
	{
		var block = GetBlockOperation(@"
			class TestClass
			{
				void TestMethod()
				{
					int value = 1;
					switch (value)
					{
						case 42:
							break;
					}
				}
			}
		");

		var walker = new SemanticWalker(true);
		var switchOp = GetOperationAt<ISwitchOperation>(block, 1);
		var caseClause = switchOp.Cases[0].Clauses[0] as ISingleValueCaseClauseOperation;

		var node = walker.VisitSingleValueCaseClause(caseClause!, new());
		var script = node?.ToKnRECMAScript();

		Assert.AreEqual("42", script);
	}

	/// <summary>
	/// 测试 VisitSingleValueCaseClause 方法（字符串字面量）
	/// </summary>
	[TestMethod]
	public void VisitSingleValueCaseClause_String()
	{
		var block = GetBlockOperation(@"
			class TestClass
			{
				void TestMethod()
				{
					string value = ""hello"";
					switch (value)
					{
						case ""test"":
							break;
					}
				}
			}
		");

		var walker = new SemanticWalker(true);
		var switchOp = GetOperationAt<ISwitchOperation>(block, 1);
		var caseClause = switchOp.Cases[0].Clauses[0] as ISingleValueCaseClauseOperation;

		var node = walker.VisitSingleValueCaseClause(caseClause!, new());
		var script = node?.ToKnRECMAScript();

		// 实际输出是 "test"（双引号+test+双引号）
		Assert.AreEqual(@"""test""", script);
	}

	/// <summary>
	/// 测试 VisitSingleValueCaseClause 方法（布尔字面量）
	/// </summary>
	[TestMethod]
	public void VisitSingleValueCaseClause_Boolean()
	{
		var block = GetBlockOperation(@"
			class TestClass
			{
				void TestMethod()
				{
					bool value = true;
					switch (value)
					{
						case false:
							break;
					}
				}
			}
		");

		var walker = new SemanticWalker(true);
		var switchOp = GetOperationAt<ISwitchOperation>(block, 1);
		var caseClause = switchOp.Cases[0].Clauses[0] as ISingleValueCaseClauseOperation;

		var node = walker.VisitSingleValueCaseClause(caseClause!, new());
		var script = node?.ToKnRECMAScript();

		Assert.AreEqual("false", script);
	}

	#region 扩展测试用例 - 多值case

	/// <summary>
	/// 测试 switch 语句 - 多值 fallthrough
	/// </summary>
	[TestMethod]
	public void VisitSwitch_MultipleFallthrough()
	{
		var block = GetBlockOperation(@"
			class TestClass
			{
				void TestMethod()
				{
					int value = 1;
					switch (value)
					{
						case 1:
						case 2:
						case 3:
							break;
					}
				}
			}
		");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		Assert.AreEqual(
			@"{
  let value = 1;
  switch (value) {
    case 1:
    case 2:
    case 3:
      break;
  }
}".ReplaceLineEndings(), script?.ReplaceLineEndings());
	}

	/// <summary>
	/// 测试 switch 语句 - 多值带语句
	/// </summary>
	[TestMethod]
	public void VisitSwitch_MultipleFallthroughWithStatements()
	{
		var block = GetBlockOperation(@"
			class TestClass
			{
				void TestMethod()
				{
					int value = 1;
					int result = 0;
					switch (value)
					{
						case 1:
						case 2:
							result = 100;
							break;
						case 3:
							result = 200;
							break;
					}
				}
			}
		");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		Assert.AreEqual(
			@"{
  let value = 1;
  let result = 0;
  switch (value) {
    case 1:
    case 2:
      result = 100;
      break;
    case 3:
      result = 200;
      break;
  }
}".ReplaceLineEndings(), script?.ReplaceLineEndings());
	}

	#endregion

	#region 扩展测试用例 - Switch表达式

	/// <summary>
	/// 测试 switch 表达式 - 简单常量
	/// </summary>
	[TestMethod]
	public void VisitSwitchExpression_SimpleConstants()
	{
		var block = GetBlockOperation(@"
			class TestClass
			{
				void TestMethod()
				{
					int value = 1;
					string result = value switch
					{
						1 => ""one"",
						2 => ""two"",
						_ => ""other""
					};
				}
			}
		");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		Assert.AreEqual(
			@"{
  let value = 1;
  let result = (() => {
    const v$0 = value;
    if (v$0 === 1)
      return ""one"";
    if (v$0 === 2)
      return ""two"";
    return ""other"";
  })();
}", script);
	}

	/// <summary>
	/// 测试 switch 表达式 - 整数返回值
	/// </summary>
	[TestMethod]
	public void VisitSwitchExpression_IntegerReturn()
	{
		var block = GetBlockOperation(@"
			class TestClass
			{
				void TestMethod()
				{
					string value = ""a"";
					int result = value switch
					{
						""a"" => 1,
						""b"" => 2,
						_ => 0
					};
				}
			}
		");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		Assert.AreEqual(
			@"{
  let value = ""a"";
  let result = (() => {
    const v$0 = value;
    if (v$0 === ""a"")
      return 1;
    if (v$0 === ""b"")
      return 2;
    return 0;
  })();
}", script);
	}

	/// <summary>
	/// 测试 switch 表达式 - 关系模式
	/// </summary>
	[TestMethod]
	public void VisitSwitchExpression_RelationalPattern()
	{
		var block = GetBlockOperation(@"
			class TestClass
			{
				void TestMethod()
				{
					int value = 5;
					string result = value switch
					{
						< 0 => ""negative"",
						0 => ""zero"",
						> 0 and < 10 => ""small"",
						_ => ""large""
					};
				}
			}
		");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		Assert.AreEqual(
			@"{
  let value = 5;
  let result = (() => {
    const v$0 = value;
    if (v$0 < 0)
      return ""negative"";
    if (v$0 === 0)
      return ""zero"";
    if (v$0 > 0 && v$0 < 10)
      return ""small"";
    return ""large"";
  })();
}", script);
	}

	/// <summary>
	/// 测试 switch 表达式 - 属性模式
	/// </summary>
	[TestMethod]
	public void VisitSwitchExpression_PropertyPattern()
	{
		var block = GetBlockOperation(@"
			class TestClass
			{
				void TestMethod()
				{
					var point = new { X = 0, Y = 0 };
					string result = point switch
					{
						{ X: 0, Y: 0 } => ""origin"",
						{ X: var x } when x > 0 => ""right"",
						_ => ""other""
					};
				}
			}
		");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		AssertScriptEqual(
			@"{
  let point = { X: 0, Y: 0 };
  let result = (() => {
    let x;
    const v$0 = point;
    if (v$0 != null && (""X"" in v$0 && v$0.X === 0) && (v$0 != null && (""Y"" in v$0 && v$0.Y === 0)))
      return ""origin"";
    if (v$0 != null && (""X"" in v$0 && (x = v$0.X, true)) && x > 0)
      return ""right"";
    return ""other"";
  })();
}", script);
	}

	#endregion

	#region 扩展测试用例 - Switch嵌套

	/// <summary>
	/// 测试嵌套 switch 语句
	/// </summary>
	[TestMethod]
	public void VisitSwitch_NestedSwitch()
	{
		var block = GetBlockOperation(@"
			class TestClass
			{
				void TestMethod()
				{
					int outer = 1;
					int inner = 2;
					switch (outer)
					{
						case 1:
							switch (inner)
							{
								case 1:
									break;
								case 2:
									break;
							}
							break;
						case 2:
							break;
					}
				}
			}
		");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		Assert.AreEqual(
			@"{
  let outer = 1;
  let inner = 2;
  switch (outer) {
    case 1:
      switch (inner) {
        case 1:
          break;
        case 2:
          break;
      }
      break;
    case 2:
      break;
  }
}", script);
	}

	/// <summary>
	/// 测试 switch 在循环中
	/// </summary>
	[TestMethod]
	public void VisitSwitch_InLoop()
	{
		var block = GetBlockOperation(@"
			class TestClass
			{
				void TestMethod()
				{
					for (int i = 0; i < 3; i++)
					{
						switch (i)
						{
							case 0:
								break;
							case 1:
								break;
							default:
								break;
						}
					}
				}
			}
		");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		Assert.AreEqual(
			@"{
  for (let i = 0; i < 3; i++) {
    switch (i) {
      case 0:
        break;
      case 1:
        break;
      default:
        break;
    }
  }
}", script);
	}

	/// <summary>
	/// 测试 switch 在 if 中
	/// </summary>
	[TestMethod]
	public void VisitSwitch_InIf()
	{
		var block = GetBlockOperation(@"
			class TestClass
			{
				void TestMethod()
				{
					int value = 1;
					bool flag = true;
					if (flag)
					{
						switch (value)
						{
							case 1:
								break;
							default:
								break;
						}
					}
				}
			}
		");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		Assert.AreEqual(
			@"{
  let value = 1;
  let flag = true;
  if (flag) {
    switch (value) {
      case 1:
        break;
      default:
        break;
    }
  }
}", script);
	}

	#endregion

	#region 扩展测试用例 - Switch返回值

	/// <summary>
	/// 测试 switch 中的 return
	/// </summary>
	[TestMethod]
	public void VisitSwitch_WithReturn()
	{
		var block = GetBlockOperation(@"
			class TestClass
			{
				int TestMethod()
				{
					int value = 1;
					switch (value)
					{
						case 1:
							return 100;
						case 2:
							return 200;
						default:
							return 0;
					}
				}
			}
		");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		Assert.AreEqual(
			@"{
  let value = 1;
  switch (value) {
    case 1:
      return 100;
    case 2:
      return 200;
    default:
      return 0;
  }
}", script);
	}

	/// <summary>
	/// 测试 switch 表达式作为参数
	/// </summary>
	[TestMethod]
	public void VisitSwitchExpression_AsArgument()
	{
		var block = GetBlockOperation(@"
			class TestClass
			{
				void TestMethod()
				{
					int value = 1;
					Console.WriteLine(value switch
					{
						1 => ""one"",
						_ => ""other""
					});
				}
			}
		");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		Assert.AreEqual(
			@"{
  let value = 1;
  console.log((() => {
    const v$0 = value;
    if (v$0 === 1)
      return ""one"";
    return ""other"";
  })());
}", script);
	}

	#endregion

	#region 扩展测试用例 - Switch类型模式

	/// <summary>
	/// 测试 switch 类型模式带声明
	/// </summary>
	[TestMethod]
	public void VisitSwitch_TypePatternWithDeclaration()
	{
		var block = GetBlockOperation(@"
			class TestClass
			{
				void TestMethod()
				{
					object obj = ""hello"";
					switch (obj)
					{
						case string s:
							Console.WriteLine(s.Length);
							break;
						case int i:
							Console.WriteLine(i);
							break;
						default:
							break;
					}
				}
			}
		");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		Assert.AreEqual(
			@"{
  let obj = ""hello"";
  (() => {
    let s, i;
    const v$0 = obj;
    if (typeof v$0 === ""string"" && (s = v$0, true)) {
      console.log(s.length);
      return;
    }
    if (typeof v$0 === ""number"" && (i = v$0, true)) {
      console.log(i);
      return;
    }
    return;
  })();
}", script);
	}

	[TestMethod]
	public void VisitSwitch_TypePatternWithEcmascriptArrayDeclaration()
	{
		var block = GetBlockOperation(@"
			class TestClass
			{
				void TestMethod()
				{
					object obj = new string[] { ""a"", ""b"" };
					switch (obj)
					{
						case Array<string> many:
							Console.WriteLine(many.Length);
							break;
						case string single:
							Console.WriteLine(single.Length);
							break;
						default:
							break;
					}
				}
			}
		");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		Assert.AreEqual(
			@"{
  let obj = [""a"", ""b""];
  (() => {
    let many, single;
    const v$0 = obj;
    if (Array.isArray(v$0) && (many = v$0, true)) {
      console.log(many.length);
      return;
    }
    if (typeof v$0 === ""string"" && (single = v$0, true)) {
      console.log(single.length);
      return;
    }
    return;
  })();
}", script);
	}

	/// <summary>
	/// 测试 switch with when 子句
	/// </summary>
	[TestMethod]
	public void VisitSwitch_WithWhenClause()
	{
		var block = GetBlockOperation(@"
			class TestClass
			{
				void TestMethod()
				{
					int value = 5;
					switch (value)
					{
						case int n when n > 0:
							Console.WriteLine(""positive"");
							break;
						case int n when n < 0:
							Console.WriteLine(""negative"");
							break;
						default:
							Console.WriteLine(""zero"");
							break;
					}
				}
			}
		");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		Assert.AreEqual(
			@"{
  let value = 5;
  (() => {
    let n;
    const v$0 = value;
    if (typeof v$0 === ""number"" && (n = v$0, true) && n > 0) {
      console.log(""positive"");
      return;
    }
    if (typeof v$0 === ""number"" && (n = v$0, true) && n < 0) {
      console.log(""negative"");
      return;
    }
    console.log(""zero"");
    return;
  })();
}", script);
	}

	#endregion

	#region 扩展测试用例 - 边界情况

	/// <summary>
	/// 测试 switch 空case数量
	/// </summary>
	[TestMethod]
	public void VisitSwitch_ManyEmptyCases()
	{
		var block = GetBlockOperation(@"
			class TestClass
			{
				void TestMethod()
				{
					int value = 1;
					switch (value)
					{
						case 1:
						case 2:
						case 3:
						case 4:
						case 5:
							break;
					}
				}
			}
		");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		Assert.AreEqual(
			@"{
  let value = 1;
  switch (value) {
    case 1:
    case 2:
    case 3:
    case 4:
    case 5:
      break;
  }
}".ReplaceLineEndings(), script?.ReplaceLineEndings());
	}

	/// <summary>
	/// 测试 switch 表达式嵌套三元运算符
	/// </summary>
	[TestMethod]
	public void VisitSwitchExpression_WithTernary()
	{
		var block = GetBlockOperation(@"
			class TestClass
			{
				void TestMethod()
				{
					int value = 1;
					string result = value switch
					{
						1 => true ? ""yes"" : ""no"",
						_ => ""other""
					};
				}
			}
		");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		Assert.AreEqual(
			@"{
  let value = 1;
  let result = (() => {
    const v$0 = value;
    if (v$0 === 1)
      return true ? ""yes"" : ""no"";
    return ""other"";
  })();
}", script);
	}

	/// <summary>
	/// 测试 switch 字符switch
	/// </summary>
	[TestMethod]
	public void VisitSwitch_CharValue()
	{
		var block = GetBlockOperation(@"
			class TestClass
			{
				void TestMethod()
				{
					char c = 'a';
					switch (c)
					{
						case 'a':
							break;
						case 'b':
							break;
						default:
							break;
					}
				}
			}
		");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		Assert.AreEqual(
			@"{
  let c = ""a"";
  switch (c) {
    case ""a"":
      break;
    case ""b"":
      break;
    default:
      break;
  }
}", script);
	}

	/// <summary>
	/// 测试 switch 枚举值
	/// </summary>
	[TestMethod]
	public void VisitSwitch_EnumValue()
	{
		var block = GetBlockOperation(@"
			class TestClass
			{
				enum Color { Red, Green, Blue }
				void TestMethod()
				{
					Color c = Color.Red;
					switch (c)
					{
						case Color.Red:
							break;
						case Color.Green:
							break;
						default:
							break;
					}
				}
			}
		");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		Assert.AreEqual(
			@"{
  let c = 0;
  switch (c) {
    case 0:
      break;
    case 1:
      break;
    default:
      break;
  }
}", script);
	}

	/// <summary>
	/// 测试 switch 多个枚举值
	/// </summary>
	[TestMethod]
	public void VisitSwitch_MultipleEnumValues()
	{
		var block = GetBlockOperation(@"
			class TestClass
			{
				enum Status { Pending, Active, Completed, Failed }
				void TestMethod()
				{
					Status s = Status.Active;
					int result = 0;
					switch (s)
					{
						case Status.Pending:
							result = 1;
							break;
						case Status.Active:
							result = 2;
							break;
						case Status.Completed:
							result = 3;
							break;
						default:
							result = 0;
							break;
					}
				}
			}
		");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		Assert.AreEqual(
			@"{
  let s = 1;
  let result = 0;
  switch (s) {
    case 0:
      result = 1;
      break;
    case 1:
      result = 2;
      break;
    case 2:
      result = 3;
      break;
    default:
      result = 0;
      break;
  }
}", script);
	}

	#endregion

	#region 扩展测试用例 - 字符串switch

	/// <summary>
	/// 测试 switch 字符串值
	/// </summary>
	[TestMethod]
	public void VisitSwitch_StringValue_Simple()
	{
		var block = GetBlockOperation(@"
			class TestClass
			{
				void TestMethod()
				{
					string name = ""test"";
					int result = 0;
					switch (name)
					{
						case ""test"":
							result = 1;
							break;
						case ""other"":
							result = 2;
							break;
					}
				}
			}
		");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		Assert.AreEqual(
			@"{
  let name = ""test"";
  let result = 0;
  switch (name) {
    case ""test"":
      result = 1;
      break;
    case ""other"":
      result = 2;
      break;
  }
}", script);
	}

	/// <summary>
	/// 测试 switch 字符串带 default
	/// </summary>
	[TestMethod]
	public void VisitSwitch_StringValue_WithDefault()
	{
		var block = GetBlockOperation(@"
			class TestClass
			{
				void TestMethod()
				{
					string mode = ""debug"";
					string output = """";
					switch (mode)
					{
						case ""debug"":
							output = ""D"";
							break;
						case ""release"":
							output = ""R"";
							break;
						default:
							output = ""?"";
							break;
					}
				}
			}
		");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		Assert.AreEqual(
			@"{
  let mode = ""debug"";
  let output = """";
  switch (mode) {
    case ""debug"":
      output = ""D"";
      break;
    case ""release"":
      output = ""R"";
      break;
    default:
      output = ""?"";
      break;
  }
}", script);
	}

	/// <summary>
	/// 测试 switch 空字符串
	/// </summary>
	[TestMethod]
	public void VisitSwitch_EmptyString()
	{
		var block = GetBlockOperation(@"
			class TestClass
			{
				void TestMethod()
				{
					string s = """";
					int result = 0;
					switch (s)
					{
						case """":
							result = 1;
							break;
						default:
							result = 0;
							break;
					}
				}
			}
		");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		Assert.AreEqual(
			@"{
  let s = """";
  let result = 0;
  switch (s) {
    case """":
      result = 1;
      break;
    default:
      result = 0;
      break;
  }
}", script);
	}

	#endregion

	#region 扩展测试用例 - 长switch

	/// <summary>
	/// 测试长 switch 语句
	/// </summary>
	[TestMethod]
	public void VisitSwitch_LongSwitch()
	{
		var block = GetBlockOperation(@"
			class TestClass
			{
				void TestMethod()
				{
					int value = 3;
					int result = 0;
					switch (value)
					{
						case 1: result = 1; break;
						case 2: result = 2; break;
						case 3: result = 3; break;
						case 4: result = 4; break;
						case 5: result = 5; break;
						default: result = -1; break;
					}
				}
			}
		");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		Assert.AreEqual(
			@"{
  let value = 3;
  let result = 0;
  switch (value) {
    case 1:
      result = 1;
      break;
    case 2:
      result = 2;
      break;
    case 3:
      result = 3;
      break;
    case 4:
      result = 4;
      break;
    case 5:
      result = 5;
      break;
    default:
      result = -1;
      break;
  }
}", script);
	}

	#endregion

	#region 扩展测试用例 - switch表达式

	/// <summary>
	/// 测试 switch 表达式带多个条件
	/// </summary>
	[TestMethod]
	public void VisitSwitchExpression_MultipleConditions()
	{
		var block = GetBlockOperation(@"
			class TestClass
			{
				void TestMethod()
				{
					int x = 10;
					string result = x switch
					{
						0 => ""zero"",
						1 => ""one"",
						2 => ""two"",
						3 => ""three"",
						_ => ""many""
					};
				}
			}
		");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		Assert.AreEqual(
			@"{
  let x = 10;
  let result = (() => {
    const v$0 = x;
    if (v$0 === 0)
      return ""zero"";
    if (v$0 === 1)
      return ""one"";
    if (v$0 === 2)
      return ""two"";
    if (v$0 === 3)
      return ""three"";
    return ""many"";
  })();
}", script);
	}

	/// <summary>
	/// 测试 switch 表达式带元组
	/// </summary>
	[TestMethod]
	public void VisitSwitchExpression_TuplePattern()
	{
		var block = GetBlockOperation(@"
			class TestClass
			{
				void TestMethod()
				{
					var (a, b) = (1, 2);
					string result = (a, b) switch
					{
						(0, 0) => ""origin"",
						(0, _) => ""y-axis"",
						(_, 0) => ""x-axis"",
						_ => ""elsewhere""
					};
				}
			}
		");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		Assert.AreEqual(
			@"{
  let a, b;
  a = 1, b = 2;
  let result = (() => {
    const v$0 = { a: a, b: b };
    if (v$0.a === 0 && v$0.b === 0)
      return ""origin"";
    if (v$0.a === 0)
      return ""y-axis"";
    if (v$0.b === 0)
      return ""x-axis"";
    return ""elsewhere"";
  })();
}".ReplaceLineEndings(), script?.ReplaceLineEndings());
	}

	/// <summary>
	/// 测试 switch 表达式带 or 模式
	/// </summary>
	[TestMethod]
	public void VisitSwitchExpression_OrPattern()
	{
		var block = GetBlockOperation(@"
			class TestClass
			{
				void TestMethod()
				{
					int value = 1;
					string result = value switch
					{
						1 or 2 or 3 => ""small"",
						4 or 5 or 6 => ""medium"",
						_ => ""large""
					};
				}
			}
		");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		Assert.AreEqual(
			@"{
  let value = 1;
  let result = (() => {
    const v$0 = value;
    if (v$0 === 1 || v$0 === 2 || v$0 === 3)
      return ""small"";
    if (v$0 === 4 || v$0 === 5 || v$0 === 6)
      return ""medium"";
    return ""large"";
  })();
}", script);
	}

	#endregion

	#region 扩展测试用例 - switch with return

	/// <summary>
	/// 测试 switch 带 return
	/// </summary>
	[TestMethod]
	public void VisitSwitch_WithReturn1()
	{
		var block = GetBlockOperation(@"
			class TestClass
			{
				int TestMethod()
				{
					int value = 1;
					switch (value)
					{
						case 1: return 10;
						case 2: return 20;
						default: return 0;
					}
				}
			}
		");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		Assert.AreEqual(
			@"{
  let value = 1;
  switch (value) {
    case 1:
      return 10;
    case 2:
      return 20;
    default:
      return 0;
  }
}", script);
	}

	/// <summary>
	/// 测试 switch 在方法中带 return
	/// </summary>
	[TestMethod]
	public void VisitSwitch_InMethodWithReturn()
	{
		var block = GetBlockOperation(@"
			class TestClass
			{
				string GetGrade(int score)
				{
					switch (score)
					{
						case int s when s >= 90: return ""A"";
						case int s when s >= 80: return ""B"";
						case int s when s >= 70: return ""C"";
						default: return ""F"";
					}
				}
			}
		");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		Assert.AreEqual(
			@"{
  (() => {
    let s;
    const v$0 = score;
    if (typeof v$0 === ""number"" && (s = v$0, true) && s >= 90) {
      return ""A"";
    }
    if (typeof v$0 === ""number"" && (s = v$0, true) && s >= 80) {
      return ""B"";
    }
    if (typeof v$0 === ""number"" && (s = v$0, true) && s >= 70) {
      return ""C"";
    }
    return ""F"";
  })();
}".ReplaceLineEndings(), script?.ReplaceLineEndings());
	}

	#endregion

	#region 扩展测试用例 - 空switch

	/// <summary>
	/// 测试空 switch 语句
	/// </summary>
	[TestMethod]
	public void VisitSwitch_EmptyBody()
	{
		var block = GetBlockOperation(@"
			class TestClass
			{
				void TestMethod()
				{
					int value = 1;
					switch (value)
					{
					}
				}
			}
		");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		Assert.AreEqual(
			@"{
  let value = 1;
  switch (value) { }
}".ReplaceLineEndings(), script?.ReplaceLineEndings());
	}

	/// <summary>
	/// 测试 switch 只有 default
	/// </summary>
	[TestMethod]
	public void VisitSwitch_OnlyDefault1()
	{
		var block = GetBlockOperation(@"
			class TestClass
			{
				void TestMethod()
				{
					int value = 1;
					switch (value)
					{
						default:
							break;
					}
				}
			}
		");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		Assert.AreEqual(
			@"{
  let value = 1;
  switch (value) {
    default:
      break;
  }
}", script);
	}

	#endregion

	#region 扩展测试用例 - 带表达式的switch

	/// <summary>
	/// 测试 switch 表达式 - 简单返回
	/// </summary>
	[TestMethod]
	public void VisitSwitch_ExpressionReturn()
	{
		var block = GetBlockOperation(@"
			class TestClass
			{
				int TestMethod()
				{
					int value = 1;
					return value switch
					{
						1 => 100,
						2 => 200,
						_ => 0
					};
				}
			}
		");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		Assert.AreEqual(
			@"{
  let value = 1;
  return (() => {
    const v$0 = value;
    if (v$0 === 1)
      return 100;
    if (v$0 === 2)
      return 200;
    return 0;
  })();
}".ReplaceLineEndings(), script?.ReplaceLineEndings());
	}

	/// <summary>
	/// 测试 switch 语句 - 多个case相同代码
	/// </summary>
	[TestMethod]
	public void VisitSwitch_MultipleCasesSameCode()
	{
		var block = GetBlockOperation(@"
			class TestClass
			{
				void TestMethod()
				{
					int value = 1;
					switch (value)
					{
						case 1:
						case 2:
						case 3:
							Console.WriteLine(""small"");
							break;
						default:
							Console.WriteLine(""large"");
							break;
					}
				}
			}
		");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		Assert.AreEqual(
			@"{
  let value = 1;
  switch (value) {
    case 1:
    case 2:
    case 3:
      console.log(""small"");
      break;
    default:
      console.log(""large"");
      break;
  }
}".ReplaceLineEndings(), script?.ReplaceLineEndings());
	}

	/// <summary>
	/// 测试 switch 表达式 - 嵌套元组
	/// </summary>
	[TestMethod]
	public void VisitSwitch_ExpressionNestedTuple()
	{
		var block = GetBlockOperation(@"
			class TestClass
			{
				string TestMethod(int a, int b)
				{
					return (a, b) switch
					{
						(0, 0) => ""both zero"",
						(0, _) => ""a zero"",
						(_, 0) => ""b zero"",
						_ => ""neither zero""
					};
				}
			}
		");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		Assert.AreEqual(
			@"{
  return (() => {
    const v$0 = { a: a, b: b };
    if (v$0.a === 0 && v$0.b === 0)
      return ""both zero"";
    if (v$0.a === 0)
      return ""a zero"";
    if (v$0.b === 0)
      return ""b zero"";
    return ""neither zero"";
  })();
}".ReplaceLineEndings(), script?.ReplaceLineEndings());
	}

	/// <summary>
	/// 测试 switch 语句 - case中使用方法调用
	/// </summary>
	[TestMethod]
	public void VisitSwitch_CaseWithMethodCall()
	{
		var block = GetBlockOperation(@"
			class TestClass
			{
				void TestMethod(string input)
				{
					switch (input.ToLower())
					{
						case ""yes"":
							Console.WriteLine(""YES"");
							break;
						case ""no"":
							Console.WriteLine(""NO"");
							break;
					}
				}
			}
		");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		Assert.AreEqual(
			@"{
  switch (input.toLowerCase()) {
    case ""yes"":
      console.log(""YES"");
      break;
    case ""no"":
      console.log(""NO"");
      break;
  }
}".ReplaceLineEndings(), script?.ReplaceLineEndings());
	}

	/// <summary>
	/// 测试 switch 表达式 - 使用属性模式
	/// </summary>
	[TestMethod]
	public void VisitSwitch_ExpressionPropertyPattern()
	{
		var block = GetBlockOperation(@"
			class TestClass
			{
				class Person { public string Name { get; set; } }

				string TestMethod(Person p)
				{
					return p switch
					{
						{ Name: ""Alice"" } => ""Hi Alice"",
						{ Name: ""Bob"" } => ""Hey Bob"",
						_ => ""Hello stranger""
					};
				}
			}
		");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		AssertScriptEqual(
			@"{
  return (() => {
    const v$0 = p;
    if (v$0 instanceof Person && (v$0 != null && (""Name"" in v$0 && v$0.Name === ""Alice"")))
      return ""Hi Alice"";
    if (v$0 instanceof Person && (v$0 != null && (""Name"" in v$0 && v$0.Name === ""Bob"")))
      return ""Hey Bob"";
    return ""Hello stranger"";
  })();
}".ReplaceLineEndings(), script?.ReplaceLineEndings());
	}

	/// <summary>
	/// 测试 switch 语句 - 使用when条件
	/// </summary>
	[TestMethod]
	public void VisitSwitch_WhenCondition()
	{
		var block = GetBlockOperation(@"
			class TestClass
			{
				void TestMethod(int value)
				{
					switch (value)
					{
						case int n when n > 0:
							Console.WriteLine(""positive"");
							break;
						case int n when n < 0:
							Console.WriteLine(""negative"");
							break;
						default:
							Console.WriteLine(""zero"");
							break;
					}
				}
			}
		");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		Assert.AreEqual(
			@"{
  (() => {
    let n;
    const v$0 = value;
    if (typeof v$0 === ""number"" && (n = v$0, true) && n > 0) {
      console.log(""positive"");
      return;
    }
    if (typeof v$0 === ""number"" && (n = v$0, true) && n < 0) {
      console.log(""negative"");
      return;
    }
    console.log(""zero"");
    return;
  })();
}".ReplaceLineEndings(), script?.ReplaceLineEndings());
	}

	[TestMethod]
	public void VisitSwitchExpression_RecordPropertyPattern_UsesStructuralMatch()
	{
		var block = GetBlockOperation(@"
			class TestClass
			{
				record Person(string Name, int Age);

				string TestMethod(Person p)
				{
					return p switch
					{
						{ Name: ""Alice"" } => ""Hi Alice"",
						{ Name: ""Bob"" } => ""Hey Bob"",
						_ => ""Hello stranger""
					};
				}
			}
		");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		AssertScriptEqual(
			@"{
  return (() => {
    const v$0 = p;
    if (v$0 != null && (""name"" in v$0 && v$0.name === ""Alice""))
      return ""Hi Alice"";
    if (v$0 != null && (""name"" in v$0 && v$0.name === ""Bob""))
      return ""Hey Bob"";
    return ""Hello stranger"";
  })();
}".ReplaceLineEndings(), script?.ReplaceLineEndings());
	}

	/// <summary>
	/// 测试 switch 表达式 - 使用关系模式
	/// </summary>
	[TestMethod]
	public void VisitSwitch_ExpressionRelationalPattern()
	{
		var block = GetBlockOperation(@"
			class TestClass
			{
				string TestMethod(int value)
				{
					return value switch
					{
						< 0 => ""negative"",
						0 => ""zero"",
						> 0 and < 10 => ""small positive"",
						>= 10 => ""large positive""
					};
				}
			}
		");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		Assert.AreEqual(
			@"{
  return (() => {
    const v$0 = value;
    if (v$0 < 0)
      return ""negative"";
    if (v$0 === 0)
      return ""zero"";
    if (v$0 > 0 && v$0 < 10)
      return ""small positive"";
    if (v$0 >= 10)
      return ""large positive"";
  })();
}".ReplaceLineEndings(), script?.ReplaceLineEndings());
	}

	/// <summary>
	/// 测试 switch 表达式 - 使用逻辑模式
	/// </summary>
	[TestMethod]
	public void VisitSwitch_ExpressionLogicalPattern()
	{
		var block = GetBlockOperation(@"
			class TestClass
			{
				string TestMethod(int value)
				{
					return value switch
					{
						1 or 2 or 3 => ""one two three"",
						4 and >= 0 => ""four"",
						not 5 => ""not five"",
						_ => ""five""
					};
				}
			}
		");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		Assert.AreEqual(
			@"{
  return (() => {
    const v$0 = value;
    if (v$0 === 1 || v$0 === 2 || v$0 === 3)
      return ""one two three"";
    if (v$0 === 4 && v$0 >= 0)
      return ""four"";
    if (!(v$0 === 5))
      return ""not five"";
    return ""five"";
  })();
}".ReplaceLineEndings(), script?.ReplaceLineEndings());
	}

	/// <summary>
	/// 测试 switch 语句 - 带返回值的表达式
	/// </summary>
	[TestMethod]
	public void VisitSwitch_ExpressionWithCalculation()
	{
		var block = GetBlockOperation(@"
			class TestClass
			{
				int TestMethod(string op, int a, int b)
				{
					return op switch
					{
						""+"" => a + b,
						""-"" => a - b,
						""*"" => a * b,
						""/"" => a / b,
						_ => 0
					};
				}
			}
		");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		Assert.AreEqual(
			@"{
  return (() => {
    const v$0 = op;
    if (v$0 === ""+"")
      return a + b;
    if (v$0 === ""-"")
      return a - b;
    if (v$0 === ""*"")
      return a * b;
    if (v$0 === ""/"")
      return a / b;
    return 0;
  })();
}".ReplaceLineEndings(), script?.ReplaceLineEndings());
	}

	/// <summary>
	/// 测试 switch 语句 - 带break和return混合
	/// </summary>
	[TestMethod]
	public void VisitSwitch_BreakReturn()
	{
		var block = GetBlockOperation(@"
			class TestClass
			{
				int TestMethod(int value)
				{
					switch (value)
					{
						case 0:
							return 0;
						case 1:
							break;
						default:
							return -1;
					}
					return 1;
				}
			}
		");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		Assert.AreEqual(
			@"{
  switch (value) {
    case 0:
      return 0;
    case 1:
      break;
    default:
      return -1;
  }
  return 1;
}".ReplaceLineEndings(), script?.ReplaceLineEndings());
	}

	#endregion
}
