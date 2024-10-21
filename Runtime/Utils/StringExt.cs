namespace io.github.azukimochi;

internal static class StringExt
{
    public static bool Contains(this string[] strings, string value, StringComparison comparisonType = StringComparison.OrdinalIgnoreCase)
    {
        foreach (string s in strings)
        {
            if (s.Equals(value, comparisonType))
                return true;
        }
        return false;
    }

    public static int IndexOf(this string[] strings, string value, StringComparison comparisonType = StringComparison.OrdinalIgnoreCase)
    {
        for (int i = 0; i < strings.Length; i++)
        {
            string s = strings[i];

            if (s.Equals(value, comparisonType))
                return i;
        }
        return -1;
    }

    public static int SplitAny(this ReadOnlySpan<char> span, Span<Range> ranges, ReadOnlySpan<char> values)
    {
        int start = 0;
        int count = 0;
        int idx;
        while ((idx = span[start..].IndexOfAny(values)) != -1)
        {
            if (ranges.Length + 1 < count)
                break;
            ranges[count++] = start..(start + idx);
            start += idx + 1;
        }
        ranges[count++] = start..;
        return count;
    }
}