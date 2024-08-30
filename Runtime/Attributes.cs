namespace io.github.azukimochi;

[AttributeUsage(AttributeTargets.Field)]
internal sealed class ShaderFeatureAttribute : PropertyAttribute
{
    public ShaderFeatureAttribute(params string[] names) => QualifiedNames = names;

    public string[] QualifiedNames { get; }
}

[AttributeUsage(AttributeTargets.Field)]
internal sealed class MinMaxSliderAttribute : PropertyAttribute
{
    public MinMaxSliderAttribute(float min, float max) => (Min, Max) = (min, max);

    public float Min { get; }

    public float Max { get; }
}

[AttributeUsage(AttributeTargets.Field)]
internal sealed class RangeParameterAttribute : PropertyAttribute
{
    public RangeParameterAttribute(string parameterName) => ParameterName = parameterName;

    public string ParameterName { get; }
}

[AttributeUsage(AttributeTargets.Field)]
internal sealed class RangeAttribute : PropertyAttribute
{
    public RangeAttribute(float min, float max) => (Min, Max) = (min, max);

    public float Min { get; }

    public float Max { get; }
}

[AttributeUsage(AttributeTargets.Field)]
internal sealed class GeneralControlAttribute : Attribute
{
    public GeneralControlAttribute(GeneralControlType type) => Type = type;

    public GeneralControlType Type { get; }
}

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Field)]
internal sealed class MenuIconAttribute : Attribute
{
    public MenuIconAttribute(string guid) => Guid = guid;

    public string Guid { get; }
}