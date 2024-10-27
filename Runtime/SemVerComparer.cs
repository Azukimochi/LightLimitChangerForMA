using System.Collections.Generic;

namespace io.github.azukimochi;

internal sealed class SemVerComparer : IComparer<string>
{
    public static SemVerComparer Instance { get; } = new SemVerComparer();

    public int Compare(string x, string y)
    {
        return SemVer.Parse(x).CompareTo(SemVer.Parse(y));
    }
}
