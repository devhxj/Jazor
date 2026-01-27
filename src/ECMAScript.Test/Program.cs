using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Operations;
using Acornima;
using ConsoleApp1;
using ECMAScript;
using ECMAScript.Test;
using System.Text;
using System.Text.Json;


Console.WriteLine("Hello, World!");


var code = @"
class TestClass
{
    void TestMethod()
    {
        int[] array = [1, 2];
        string result = array switch
        {
            [..] => ""empty or any"",
            _ => ""other""
        };
    }
}
";

    // 创建 global using 语法树
    var usings = @"
        global using System;
        global using System.Collections.Generic;
        global using System.Linq;";

    var compilation = CSharpCompilation.Create(
        "TestAssembly",
        syntaxTrees:
        [
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


//Thread.Sleep(5000);

//// 创建客户端
//using var client = new NamedPipeClient("ECMAScript");

//var text = new FileRequest(
//	"test",
//	".js",
//	Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "test.js"),
//	"console.log('Hello, World!');"
//);

//// 准备请求消息
//var request = new PipeMessage(
//	CommandType: 1,
//	Body: Encoding.UTF8.GetBytes(JsonSerializer.Serialize(text))
//);

//try
//{
//	// 发送请求并获取响应
//	var response = await client.RequestAsync(request);

//	// 处理响应
//	Console.WriteLine($"响应{response.CommandType}: {Encoding.UTF8.GetString(response.Body)}");
//}
//catch (Exception ex)
//{
//	Console.WriteLi ne($"请求失败: {ex.Message}");
//}
var result = (1,(3,5)) == (2,(4,6));
var tuple = (outer: (inner: 1, 2), 3);
var  b = (2,4);
var  c = (2,"a");
((int bbb, int ccc),int aaa) = tuple;
var script = @"
const a = Number.MAX_SAFE_INTEGER
";
var parser = new Parser(new ParserOptions { 

});
var ast = parser.ParseModule(script);
Console.WriteLine($"CODE:{script}");
Console.WriteLine($"------------------");
Console.WriteLine($"{ast.ToJson()}");
Console.WriteLine($"------------------");
Console.WriteLine($"OUT:{ast.ToJavaScript()}");
Console.ReadLine();



