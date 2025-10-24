using Acornima;
using ConsoleApp1;
using ECMAScript.Test;
using System.Text;
using System.Text.Json;

Console.WriteLine("Hello, World!");

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
int c;
(c,int e) = (2, 2);
(int a, int b) = (3, 4/5);

var script = @"
let tuple= {a:1,Item2:2}
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

