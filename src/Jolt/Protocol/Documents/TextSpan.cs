namespace Jolt.Protocol.Documents;

public readonly record struct TextSpan(int Start, int Length)
{
    public int End => Start + Math.Max(Length, 0);
}
