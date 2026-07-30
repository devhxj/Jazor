namespace Jazor.Compiler;

/// <summary>
/// 描述某个 lowering 临时名称的逻辑拥有者。
/// </summary>
/// <remarks>
/// <see cref="StableKey"/> 用于跨发射保持名称稳定，<see cref="IdentityKey"/> 用于区分同一
/// lowering 中的不同操作。两者不能随意合并，否则不同位置可能错误复用同一个临时变量。
/// </remarks>
internal readonly record struct LoweringNameOwner(string StableKey, string IdentityKey);
