namespace io.github.azukimochi;

[AttributeUsage(AttributeTargets.Field)]
internal sealed class ShaderFeatureAttribute : Attribute
{
    public ShaderFeatureAttribute(SupportedShaders shaders) => Shaders = shaders;

    public SupportedShaders Shaders { get; }
}