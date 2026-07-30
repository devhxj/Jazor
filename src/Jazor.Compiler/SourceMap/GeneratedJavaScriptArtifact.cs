namespace Jazor.Compiler;

/// <summary>
/// 表示一次 JavaScript 发射的完整结果及其可选源码映射摘要。
/// </summary>
/// <remarks>
/// 该 record 只是 compiler 到 Emit 层的结果载体，不负责写文件、打包或发布。
/// hash 用于判断内容变化；SourceMapContent 为空时表示映射生成失败或按选项关闭。
/// </remarks>
public sealed record GeneratedJavaScriptArtifact(
    string Content,
    string? SourceMapContent,
    string JsHash,
    string? MapHash);
