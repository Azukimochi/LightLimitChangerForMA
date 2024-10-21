using System.Collections.Generic;

namespace io.github.azukimochi;

[Serializable]
public sealed class SemVer
{
    public int Major;
    public int Minor;
    public int Patch;
    public string Label;

    public SemVer(int major, int minor, int patch, string label = null)
    {
        Major = major;
        Minor = minor;
        Patch = patch;
        Label = label;
    }

    public static SemVer Parse(ReadOnlySpan<char> value)
    {
        var ranges = (stackalloc Range[6]);
        int count = value.SplitAny(ranges, ".-");
        if (count < 3)
            return default;

        var major = value[ranges[0]];
        var minor = value[ranges[1]];
        var patch = value[ranges[2]];
        var label = count < 4 ? null : value[ranges[3].Start..].ToString();
        if (label != null)
        {
            var buildMeta = label.IndexOf('+');
            if (buildMeta > 0)
            {
                label = label[..buildMeta];
            }
        }

        return new SemVer(int.Parse(major), int.Parse(minor), int.Parse(patch), label);
    }

    public int CompareTo(SemVer other)
    {
        var major = Major.CompareTo(other.Major);
        if (major != 0)
            return major;

        var minor = Minor.CompareTo(other.Minor);
        if (minor != 0)
            return minor;

        int patch = Patch.CompareTo(other.Patch);
        if (patch != 0)
            return patch;

        if (Label.Length == 0 && other.Label.Length != 0)
            return 1;
        else if (Label.Length != 0 && other.Label.Length == 0)
            return -1;
        else
            return Label.AsSpan().CompareTo(other.Label, StringComparison.OrdinalIgnoreCase);
    }

    public override string ToString() => $"{Major}.{Minor}.{Patch}{(Label.AsSpan().IsEmpty ? "" : "-")}{Label}";

}

internal sealed class SemVerComparer : IComparer<string>
{
    public static SemVerComparer Instance { get; } = new SemVerComparer();

    public int Compare(string x, string y)
    {
        return SemVer.Parse(x).CompareTo(SemVer.Parse(y));
    }
}
