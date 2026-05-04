using Microsoft.CodeAnalysis;

namespace Jazor.Compiler;

public sealed record AstConverterOptions(
    AstConverterProfile Profile,
    Func<ISymbol, bool>? MemberFilter = null)
{
    public static AstConverterOptions Default { get; } = new(AstConverterProfile.Standard);
}

public enum AstConverterProfile
{
    Standard = 0,
    ClrRuntime = 1
}
