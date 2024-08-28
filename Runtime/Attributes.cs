﻿namespace io.github.azukimochi;

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

internal sealed class RangeParameterAttribute : Attribute
{
    public RangeParameterAttribute(string parameterName) => ParameterName = parameterName;

    public string ParameterName { get; }
}

internal sealed class RangeAttribute : PropertyAttribute
{
    public RangeAttribute(float min, float max) => (Min, Max) = (min, max);

    public float Min { get; }

    public float Max { get; }
}

internal sealed class GeneralControlAttribute : Attribute
{
    public GeneralControlAttribute(GeneralControlType type) => Type = type;

    public GeneralControlType Type { get; }
}