namespace io.github.azukimochi;

/// <summary>
/// 範囲を限定する
/// </summary>
[AttributeUsage(AttributeTargets.Field)]
internal sealed class RangeAttribute : PropertyAttribute
{
    public RangeAttribute(float min, float max) => (Min, Max) = (min, max);

    public float Min { get; }

    public float Max { get; }
}
