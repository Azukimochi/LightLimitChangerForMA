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
}