using System.Text;

namespace System.Runtime.CompilerServices;

[InterpolatedStringHandler]
internal readonly ref struct StringBuilderInterpolatedStringHandler
{
    private readonly StringBuilder sb;

    public StringBuilderInterpolatedStringHandler(int literalLength, int formattedCount, StringBuilder sb)
    {
        this.sb = sb;
    }

    public StringBuilderInterpolatedStringHandler(int literalLength, int formattedCount)
    {
        sb = new(literalLength);
    }

    public void AppendLiteral(string value) => sb.Append(value);

    public void AppendFormatted(string value) => sb.Append(value);
    public void AppendFormatted(ReadOnlySpan<char> value) => sb.Append(value);
    public void AppendFormatted(int value) => sb.Append(value);
    public void AppendFormatted(char value) => sb.Append(value);
    public void AppendFormatted(bool value) => sb.Append(value);
    public void AppendFormatted<T>(T x) => sb.Append(x);

    public new string ToString() => sb.ToString();
    
    public string ToStringAndClear()
    {
        var s = sb.ToString();
        sb.Clear();
        return s;
    }
}
