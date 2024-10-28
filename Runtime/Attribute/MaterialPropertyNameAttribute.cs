namespace io.github.azukimochi;

[AttributeUsage(AttributeTargets.Field, AllowMultiple = true)]
internal sealed class MaterialPropertyNameAttribute : PropertyAttribute
{
    public MaterialPropertyNameAttribute(string shader, string name)
    {
        Shader = shader;
        Name = name;
    }

    public string Shader { get; }
    public string Name { get; }
}