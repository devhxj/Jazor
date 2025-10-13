using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace ECMAScript.Compiler;

[Generator]
public class ESGenerator : IIncrementalGenerator
{
	public void Initialize(IncrementalGeneratorInitializationContext context)
	{
		var classDecls = context.SyntaxProvider.ForAttributeWithMetadataName(
			"ECMAScript.ECMAScriptModuleAttribute",
			predicate: (node, _) => node is ClassDeclarationSyntax,
			transform: (context, _) => (context.TargetNode, context.SemanticModel)
		);

		// 转换为 JavaScript 模块
		context.RegisterImplementationSourceOutput<(SyntaxNode TargetNode, SemanticModel SemanticModel)> (
			source: classDecls,
			action: (outputContext, transform) =>
			{
				try
				{
					var classSymbol = (INamedTypeSymbol)transform.SemanticModel.GetDeclaredSymbol(transform.TargetNode)!;
                    var astConverter = new AstConverter(classSymbol, transform.SemanticModel);

					// 获取类名作为文件名
					var classDecl = (ClassDeclarationSyntax)transform.TargetNode;
					var className = classDecl.Identifier.ValueText;
					
					// 执行转换
					var jsAst = astConverter.Convert();
					
					// 生成 JavaScript 代码（简化实现）
					var jsCode = $"// Generated from {className}\n// TODO: Implement AST to JavaScript conversion\nexport class {className} {{\n  constructor() {{\n    // TODO\n  }}\n}}\n";
					
					// 输出 JavaScript 文件
					outputContext.AddSource($"{className}.mjs", jsCode);
				}
				catch (System.Exception ex)
				{
					// 处理转换异常
					var errorMsg = $"// 转换失败: {ex.Message}\n// {ex.StackTrace}";
					outputContext.AddSource("conversion.error.txt", errorMsg);
				}
			});
	}
}
