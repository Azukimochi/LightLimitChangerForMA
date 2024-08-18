namespace io.github.azukimochi;

[AttributeUsage(AttributeTargets.Field)]
internal sealed class ShaderFeatureAttribute : PropertyAttribute
{
    public ShaderFeatureAttribute(SupportedShaders shaders) => Shaders = shaders;

    public SupportedShaders Shaders { get; }
}

[AttributeUsage(AttributeTargets.Field)]
internal sealed class MinMaxSliderAttribute : PropertyAttribute
{
    public MinMaxSliderAttribute(float min, float max) => (Min, Max) = (min, max);

    public float Min { get; }

    public float Max { get; }
}

internal sealed class RangeParameterAttribute : Attribute
{
    public RangeParameterAttribute(string parameterName) => ParameterName = parameterName;

    public string ParameterName { get; }
}