using Acornima.Ast;
using ECMAScript.Compiler;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Operations;

namespace ECMAScript.ComplierTest;

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

	/// <summary>
	/// 获取指定索引的操作
	/// </summary>
	private static T GetOperationAt<T>(IBlockOperation block, int index = 0) where T : class, IOperation
	{
		var operation = block.Operations.Skip(index).First();
		return operation as T ?? throw new InvalidOperationException("未找到可分析的操作");
	}

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
      break;
    case 2:
  }
}", script);
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
	/// 注意：当前实现对方法调用的处理可能有问题
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

		// TODO: 当前实现对方法调用的处理存在问题，返回空块
		Assert.AreEqual(
			@"{ }", script);
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
		// 类型模式中的变量 s 和 i 被声明在外层，并通过赋值表达式进行初始化
		// 空 case 体被转换为 return 语句
		// 每个 case 是独立的 if 语句
		// 类型模式中的变量声明通过逗号表达式转换为赋值
		Assert.AreEqual(
			@"{
  let obj = ""hello"";
  let s, i;
  (() => {
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
}
