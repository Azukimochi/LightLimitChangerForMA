namespace io.github.azukimochi;

[Serializable]
public struct TargetShaderContainer
{
    private static readonly string[] everything = { "EVERYTHING" };
    private static readonly string[] nothing = { };

    public string[] ShaderIds = nothing;

    public TargetShaderContainer(string[] shaderIDs)
    {
        ShaderIds = shaderIDs;
    }

    public readonly bool Contains(string id, StringComparison comparisonType = StringComparison.Ordinal)
    {
        if (IsNothing)
            return false;

        if (IsEverything)
            return true;

        return ShaderIds.Contains(id, comparisonType);
    }

    public readonly bool IsEverything => (ShaderIds?.Length ?? 0) == 1 && ShaderIds[0] == everything[0];

    public readonly bool IsNothing => (ShaderIds?.Length ?? 0) == 0;

    public static TargetShaderContainer Everything => new(everything);
    public static TargetShaderContainer Nothing => new(nothing);

    public static implicit operator TargetShaderContainer(string[] shaderIDs) => new(shaderIDs);
    public static implicit operator string[](TargetShaderContainer targetShaderContainer) => targetShaderContainer.ShaderIds;
}