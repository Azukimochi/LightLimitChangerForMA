namespace io.github.azukimochi;

[AttributeUsage(AttributeTargets.Field)]
internal sealed class VectorFieldAttribute : Attribute
{
    public VectorFieldAttribute(VectorField field) => Field = field;

    public VectorField Field { get; }

    public string Group { get; set; } = "";
}

internal enum VectorField
{
    X,
    Y,
    Z,
    W
}