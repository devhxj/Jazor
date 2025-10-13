# Jazor - C# 到 JavaScript 编译器

[![.NET](https://img.shields.io/badge/.NET-10.0-blue.svg)](https://dotnet.microsoft.com/)
[![License](https://img.shields.io/badge/License-MIT-green.svg)](LICENSE)

Jazor 是一个高性能的 C# 到 JavaScript 编译器，旨在实现 C# 代码到 JavaScript 代码的语义等价转换。该项目基于 Roslyn 编译器平台，通过 AST（抽象语法树）转换技术，精确地将 C# 代码转换为可在浏览器或 Node.js 环境中运行的 JavaScript 代码。

## 主要特性

- **语义等价转换**：确保 C# 和 JavaScript 之间的语义完全等价，避免任何形式的简化处理
- **完整语法支持**：支持变量声明、控制流、函数、类、模式匹配等现代 C# 语法
- **高级模式匹配**：完整支持 C# 8.0+ 的模式匹配功能，包括递归模式、关系模式等
- **异步编程支持**：完整支持 async/await 异步编程模型
- **Web API 绑定**：自动生成 Web API 的 C# 绑定，支持 DOM、CSS、WebGL 等
- **编译时优化**：利用 C# 强类型系统的编译时信息，生成最优化的 JavaScript 代码
- **可扩展架构**：模块化设计，支持自定义转换规则和扩展

## 项目结构

```
Jazor/
├── src/
│   ├── ECMAScript/                 # 核心 ECMAScript 实现
│   │   ├── attribute/             # ECMAScript 特性定义
│   │   ├── generate/              # 自动生成的类型绑定
│   │   ├── internal/              # 内部类型实现
│   │   └── Global.cs              # 全局方法和属性
│   ├── ECMAScript.CLR/            # CLR 运行时支持
│   ├── ECMAScript.Analyzer/       # 静态代码分析器
│   ├── ECMAScript.Compiler/       # C# 到 JavaScript 编译器
│   │   └── AstOperationWalker.cs # 核心 AST 转换器
│   ├── ECMAScript.Server/         # 编译服务器
│   ├── ECMAScript.Test/           # 单元测试
│   ├── ECMAScript.ComplierTest/  # 编译器测试
│   ├── ECMAScript.WebIDL/         # Web IDL 绑定生成
│   └── ECMAScript.CLRGenerate/    # CLR 类型生成器
├── PROJECT_RULES.md               # 项目开发规则
└── README.md                      # 项目说明
```

## 核心组件

### 1. ECMAScript.Compiler
核心编译器组件，负责将 C# 的 Roslyn 操作树转换为 JavaScript 的 Acornima AST。主要特性：
- 直接 AST 构造，避免字符串解析开销
- 语义等价性保证，确保转换前后行为一致
- 完整的错误处理和异常报告机制

### 2. ECMAScript.WebIDL
Web API 绑定生成器，自动从 Web IDL 规范生成 C# 类型绑定。支持：
- DOM API 绑定
- CSS API 绑定
- WebGL API 绑定
- 现代 Web 标准 API 绑定

### 3. ECMAScript.CLRGenerate
CLR 类型生成器，为 .NET 基础类型库生成 JavaScript 绑定。包括：
- 基础类型（Object, String, Number 等）
- 集合类型（Array, Dictionary, Set 等）
- 日期时间类型处理

### 4. ECMAScript.Server
编译服务器，提供基于命名管道的编译服务：
- 支持持续编译
- 提供远程编译接口
- 集成到开发工作流

## 支持的 C# 语法

### 基础语法
- 变量声明和初始化
- 运算符（算术、逻辑、位运算）
- 控制流（if/else, switch, for, foreach, while）
- 异常处理（try/catch/finally）

### 高级语法
- Lambda 表达式和本地函数
- 异步编程（async/await）
- 模式匹配（is, switch 表达式）
- 元组和解构
- 插值字符串
- 空合并运算符
- 条件访问运算符

### 面向对象编程
- 类和结构体
- 属性和字段
- 方法和构造函数
- 继承和多态
- 接口实现

## 转换示例

### 基础代码转换
```csharp
// C# 代码
int x = 42;
string message = $"Value is {x}";
bool isPositive = x > 0;
```

```javascript
// 转换后的 JavaScript 代码
let x = 42;
let message = "Value is " + x;
let isPositive = x > 0;
```

### 模式匹配转换
```csharp
// C# 代码
string DescribeValue(int value) => value switch
{
    < 0 => "Negative",
    > 0 and < 100 => "Small Positive",
    >= 100 => "Large Positive",
    _ => "Zero"
};
```

```javascript
// 转换后的 JavaScript 代码
function describeValue(value) {
    return (() => {
        if (value < 0) return "Negative";
        if (value > 0 && value < 100) return "Small Positive";
        if (value >= 100) return "Large Positive";
        return "Zero";
    })();
}
```

### 异步编程转换
```csharp
// C# 代码
async Task<string> FetchDataAsync(string url)
{
    var client = new HttpClient();
    var response = await client.GetAsync(url);
    return await response.Content.ReadAsStringAsync();
}
```

```javascript
// 转换后的 JavaScript 代码
async function fetchDataAsync(url) {
    const response = await fetch(url);
    return await response.text();
}
```

## 使用方法

### 基本编译
```csharp
using ECMAScript.Compiler;
using Microsoft.CodeAnalysis;

// 创建编译器实例
var walker = new AstOperationWalker();

// 编译 C# 代码
var compilation = CSharpCompilation.Create("TestAssembly", 
    syntaxTrees, 
    references, 
    options);

// 获取语义模型
var semanticModel = compilation.GetSemanticModel(syntaxTree);

// 转换为 JavaScript AST
var operation = semanticModel.GetOperation(syntaxNode);
var jsAst = walker.Visit(operation, operation);

// 生成 JavaScript 代码
var jsCode = jsAst.ToJavaScript();
```

### 使用编译服务器
```csharp
using ECMAScript.Server;
using System.IO.Pipes;

// 连接到编译服务器
using var client = new NamedPipeClient("ECMAScript");

// 准备编译请求
var request = new PipeMessage(
    CommandType: 1,
    Body: Encoding.UTF8.GetBytes(csharpCode)
);

// 发送请求并获取响应
var response = await client.RequestAsync(request);
var jsCode = Encoding.UTF8.GetString(response.Body);
```

## 开发和构建

### 环境要求
- .NET 10.0 SDK 或更高版本
- Visual Studio 2022 或 Visual Studio Code
- Windows、Linux 或 macOS

### 构建步骤
```bash
# 克隆仓库
git clone https://github.com/your-repo/Jazor.git
cd Jazor

# 还原依赖
dotnet restore

# 构建解决方案
dotnet build

# 运行测试
dotnet test

# 生成 Web API 绑定
cd src/ECMAScript.WebIDL
npm install
npm run build
```

## 贡献指南

我们欢迎社区贡献！请阅读 [PROJECT_RULES.md](PROJECT_RULES.md) 了解详细的开发规则和贡献流程。

### 开发流程
1. Fork 项目仓库
2. 创建功能分支
3. 实现功能并添加测试
4. 确保所有测试通过
5. 提交 Pull Request

### 代码规范
- 遵循 C# 编码约定
- 添加适当的注释和文档
- 确保新功能有相应的单元测试
- 遵循语义等价性原则

## 许可证

本项目采用 MIT 许可证。详情请参阅 [LICENSE](LICENSE) 文件。

## 联系方式

- 项目主页：https://github.com/your-repo/Jazor
- 问题反馈：https://github.com/your-repo/Jazor/issues
- 邮箱：your-email@example.com

## 致谢

感谢所有为 Jazor 项目做出贡献的开发者和社区成员！

特别感谢以下开源项目：
- [Roslyn](https://github.com/dotnet/roslyn) - C# 编译器平台
- [Acornima](https://github.com/terrytyrius/Acornima) - JavaScript 解析器
- [WebRef](https://github.com/w3c/webref) - Web 规范参考