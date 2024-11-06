namespace io.github.azukimochi;

[AttributeUsage(AttributeTargets.Field)]
internal sealed class MinMaxRangeAttribute : Attribute
{
    public MinMaxRangeAttribute(float min, float max) => (Min, Max) = (min, max);

    public float Min { get; }

    public float Max { get; }
}